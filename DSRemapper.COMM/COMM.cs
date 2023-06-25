using DSRemapper.Core;
using DSRemapper.DSMath;
using DSRemapper.SixAxis;
using DSRemapper.Types;
using FireLibs.IO.COMPorts;
using System.Runtime.InteropServices;

namespace DSRemapper.COMM
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct COMInfoReport
    {
        public byte Axis = 0; //per controller
        public byte Buttons = 0; //per controller
        public byte Povs = 0; //per controller
        public ushort AccelScale = 0;
        public ushort GyroScale = 0;

        public COMInfoReport() { }
    };

    //Pack is required to match report length of the COM device
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct COMInputReport
    {
        public byte id = 0;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public short[] Axis = new short[12];
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] Buttons = new byte[4];
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public ushort[] Pov = new ushort[2];
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public short[] Accel = new short[3];
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public short[] Gyro = new short[3];

        public COMInputReport() { }
    };
    //Pack is required to match report length of the COM device
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct COMOutputReport
    {
        public readonly byte code = 2;
        public byte id = 0;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public short[] Motors = new short[6];
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public byte[] Leds = new byte[6];

        public COMOutputReport() { }
    };
    public class COMMScanner : IDSDeviceScanner
    {
        public IDSInputDeviceInfo[] ScanDevices()
        {
            return SerialPort.GetSerialDevices().Select(sd => new COMMDeviceInfo(sd)).ToArray();
        }
    }

    public class COMMDeviceInfo : IDSInputDeviceInfo
    {
        public string Id => Info.PortName;

        public string Name => $"Controller {Id}";

        public int VendorId => 0;

        public int ProductId => 0;

        public SerialDeviceInfo Info { get; private set; }

        public COMMDeviceInfo(SerialDeviceInfo info)
        {
            Info=info;
        }
        public IDSInputController CreateController()
        {
            return new COMM(this);
        }
    }

    public class COMM : IDSInputController
    {
        const int BaudRate = 57600;
        private static int COMInfoReportSize { get => Marshal.SizeOf(typeof(COMInfoReport)); }
        private static int COMInputReportSize { get => Marshal.SizeOf(typeof(COMInputReport)); }
        private static int COMOutputReportSize { get => Marshal.SizeOf(typeof(COMOutputReport)); }
        private static readonly byte[] infoReportRequst = new byte[] { 0x00 };
        private static readonly byte[] inputReportRequst = new byte[] { 0x01 };

        private readonly SerialPort sp;
        private COMInputReport rawReport = new();
        private DSInputReport report = new();
        private COMInfoReport? information;

        private CustomMotionProcess motPro = new();
        private ExpMovingAverageVector3 gyroAvg = new();
        private DSVector3 lastGyro = new();

        private const int readTimeout = 500;

        public string Id { get; private set; }

        public string Name => $"Controller {Id}";

        public string Type => "COMM";

        public bool IsConnected => sp.IsConnected;

        public COMM(COMMDeviceInfo info)
        {
            Id = info.Id;
            report = new(sliders: 6, buttons: 32, povs: 2);
            sp = new(info.Info, BaudRate)
            {
                ReadTimeout = readTimeout
            };
        }
        public void Connect()
        {
            if (!IsConnected)
            {
                sp.Connect();
            }
        }

        public void Disconnect()
        {
            if (IsConnected)
            {
                sp.Disconnect();
            }
        }

        public void Dispose()
        {
            Disconnect();
        }

        private COMInfoReport? ReadInfoReport()
        {
            if (sp.BytesToRead > 0)
                sp.ReadExisting();

            COMInfoReport? report = null;
            sp.Write(infoReportRequst, 0, infoReportRequst.Length);
            byte[] buffer = new byte[COMInfoReportSize];
            if (sp.ReadCount(buffer, 0, buffer.Length))
            {
                GCHandle ptr = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                report = Marshal.PtrToStructure<COMInfoReport>(ptr.AddrOfPinnedObject());
                ptr.Free();
            }

            return report;
        }

        public DSInputReport GetInputReport()
        {
            if (sp.BytesToRead > 0)
                sp.ReadExisting();

            information ??= ReadInfoReport();

            sp.Write(inputReportRequst, 0, inputReportRequst.Length);
            byte[] buffer = new byte[COMInputReportSize];

            if (sp.ReadCount(buffer, 0, buffer.Length))
            {
                GCHandle ptr = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                rawReport = Marshal.PtrToStructure<COMInputReport>(ptr.AddrOfPinnedObject());
                ptr.Free();

                report.Axis[0] = AxisToFloat(rawReport.Axis[0]);
                report.Axis[1] = AxisToFloat(rawReport.Axis[1]);
                report.Axis[2] = AxisToFloat(rawReport.Axis[2]);
                report.Axis[3] = AxisToFloat(rawReport.Axis[3]);
                report.Axis[4] = AxisToFloat(rawReport.Axis[4]);
                report.Axis[5] = AxisToFloat(rawReport.Axis[5]);

                report.Sliders[0] = AxisToFloat(rawReport.Axis[6]);
                report.Sliders[1] = AxisToFloat(rawReport.Axis[7]);
                report.Sliders[2] = AxisToFloat(rawReport.Axis[8]);
                report.Sliders[3] = AxisToFloat(rawReport.Axis[9]);
                report.Sliders[4] = AxisToFloat(rawReport.Axis[10]);
                report.Sliders[5] = AxisToFloat(rawReport.Axis[11]);

                for (int i = 0; i < report.Buttons.Length; i++)
                {
                    report.Buttons[i] = Convert.ToBoolean(rawReport.Buttons[i / 8] & (1 << i % 8));
                }

                if (rawReport.Pov[0] == ushort.MaxValue)
                    report.Povs[0].Angle = -1;
                else
                    report.Povs[0].Angle = rawReport.Pov[0] / 100f;
                report.Povs[0].CalculateButtons();
                if (rawReport.Pov[1] == ushort.MaxValue)
                    report.Povs[1].Angle = -1;
                else
                    report.Povs[1].Angle = rawReport.Pov[1] / 100f;
                report.Povs[1].CalculateButtons();

                if (information != null)
                {
                    if (information.Value.AccelScale > 0 && information.Value.GyroScale > 0)
                    {
                        float accelScale = 32768 / information.Value.AccelScale;
                        float gyroScale = 32768 / information.Value.GyroScale;
                        report.RawAccel = new DSVector3(rawReport.Accel[0] / accelScale,
                            rawReport.Accel[1] / accelScale, rawReport.Accel[2] / accelScale);
                        report.Gyro = new DSVector3(rawReport.Gyro[0] / gyroScale,
                            rawReport.Gyro[1] / gyroScale, rawReport.Gyro[2] / gyroScale);

                        DSVector3 temp = report.Gyro - lastGyro;
                        if (temp.Length < 1f)
                            gyroAvg.Update(report.Gyro, 200);

                        lastGyro = report.Gyro;

                        report.Gyro -= gyroAvg.Mean;

                        motPro.Update(report.RawAccel, report.Gyro);

                        report.Grav = -motPro.grav;
                        report.Accel = motPro.Accel;
                        report.Rotation = motPro.rotation;
                        report.DeltaRotation = motPro.deltaRotation;
                        report.deltaTime = motPro.DeltaTime;
                    }
                    else
                    {
                        report.RawAccel = new DSVector3(rawReport.Accel[0], rawReport.Accel[1], rawReport.Accel[2]);
                        report.Gyro = new DSVector3(rawReport.Gyro[0], rawReport.Gyro[1], rawReport.Gyro[2]);
                    }
                }
                else
                {
                    report.RawAccel = new DSVector3(rawReport.Accel[0], rawReport.Accel[1], rawReport.Accel[2]);
                    report.Gyro = new DSVector3(rawReport.Gyro[0], rawReport.Gyro[1], rawReport.Gyro[2]);
                }

                if (!float.IsNormal(report.RawAccel.Length))
                    report.RawAccel = new DSVector3();
                if (!float.IsNormal(report.Gyro.Length))
                    report.Gyro = new DSVector3();
                if (!float.IsNormal(report.Grav.Length))
                    report.Grav = new DSVector3();
                if (!float.IsNormal(report.Accel.Length))
                    report.Accel = new DSVector3();
            }

            return report;
        }
        public void SendOutputReport(DSOutputReport report)
        {
            COMOutputReport outReport = new();

            for (int i = 0; i < report.Rumble.Length; i++)
                outReport.Motors[i] = report.Rumble[i].ToShortAxis();
            for (int i = 0; i < report.ExtLeds.Length; i++)
                outReport.Leds[i] = (byte)report.ExtLeds[i];

            byte[] buffer = new byte[COMOutputReportSize];

            GCHandle ptr = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            Marshal.StructureToPtr(outReport, ptr.AddrOfPinnedObject(), false);
            ptr.Free();

            sp.Write(buffer, 0, buffer.Length);

            sp.ReadCount(buffer, 0, 1);
        }

        private static float AxisToFloat(int axis) => (float)axis / (short.MaxValue + (axis < 0 ? 1 : 0));
    }

    internal static class FloatExtensions
    {
        public static short ToShortAxis(this float axis) => (short)(axis * (axis < 0 ? -short.MinValue : short.MaxValue));
        public static sbyte ToSByteAxis(this float axis) => (sbyte)(axis * (axis < 0 ? -sbyte.MinValue : sbyte.MaxValue));
        public static byte ToByteTrigger(this float axis) => (byte)(axis * byte.MaxValue);
    }
}