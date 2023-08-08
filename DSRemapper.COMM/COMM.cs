using DSRemapper.Core;
using DSRemapper.DSMath;
using DSRemapper.SixAxis;
using DSRemapper.Types;
using FireLibs.IO.COMPorts.Win;
using SerialDeviceInfo = FireLibs.IO.COMPorts.SerialDeviceInfo;
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
    /// <summary>
    /// COM device scanner class
    /// </summary>
    public class COMMScanner : IDSDeviceScanner
    {
        /// <summary>
        /// Gets available/connected devices as IDSInputDeviceInfo array
        /// </summary>
        /// <returns>A IDSInputDeviceInfo array containing the COM devices</returns>
        public IDSInputDeviceInfo[] ScanDevices()
        {
            return SerialPort.GetSerialDevices().Select(sd => new COMMDeviceInfo(sd)).ToArray();
        }
    }
    /// <summary>
    /// COM device info class
    /// </summary>
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
    /// <summary>
    /// COM controller class
    /// </summary>
    public class COMM : IDSInputController
    {
        const BaudRates BaudRate = BaudRates.BR57600;
        private static int COMInfoReportSize { get => Marshal.SizeOf(typeof(COMInfoReport)); }
        private static int COMInputReportSize { get => Marshal.SizeOf(typeof(COMInputReport)); }
        private static int COMOutputReportSize { get => Marshal.SizeOf(typeof(COMOutputReport)); }
        private static readonly byte[] infoReportRequst = new byte[] { 0x00 };
        private static readonly byte[] inputReportRequst = new byte[] { 0x01 };

        private bool isNotFirstRead = false;
        private readonly SerialPort sp;
        private COMInputReport rawReport = new();
        private DSInputReport report = new();
        private COMInfoReport? information;

        private SixAxisProcess motPro = new();
        private ExpMovingAverageVector3 gyroAvg = new();
        private DSVector3 lastGyro = new();

        public string Id { get; private set; }

        public string Name => $"Controller {Id}";

        public string Type => "COMM";

        public bool IsConnected => sp.IsConnected;

        public COMM(COMMDeviceInfo info)
        {
            Id = info.Id;
            report = new(6, 6, 32, 2,0);
            sp = new(info.Info, BaudRate);
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
            sp.FlushRXBuffer();

            sp.Write(infoReportRequst);
            if(sp.Read(out COMInfoReport report)>=Marshal.SizeOf<COMInfoReport>())
                return report;
            return null;
        }

        public DSInputReport GetInputReport()
        {
            sp.FlushRXBuffer();

            information ??= ReadInfoReport();

            sp.Write(inputReportRequst);

            if(sp.Read(out rawReport)>0)
            { 
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

            sp.Write(outReport);

            sp.Read(out _, 1);
        }

        private static float AxisToFloat(int axis) => (float)axis / (short.MaxValue + (axis < 0 ? 1 : 0));
    }

    internal static class FloatExtensions
    {
        public static float ToFloatAxis(this short axis) => (float)axis / (short.MaxValue + (axis < 0 ? 1 : 0));
        public static short ToShortAxis(this float axis) => (short)(axis * (axis < 0 ? -short.MinValue : short.MaxValue));
        public static sbyte ToSByteAxis(this float axis) => (sbyte)(axis * (axis < 0 ? -sbyte.MinValue : sbyte.MaxValue));
        public static byte ToByteTrigger(this float axis) => (byte)(axis * byte.MaxValue);
    }
}