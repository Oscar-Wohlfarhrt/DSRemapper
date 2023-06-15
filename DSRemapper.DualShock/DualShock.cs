using DSRemapper.Core;
using DSRemapper.DSLogger;
using DSRemapper.DSMath;
using DSRemapper.HID;
using DSRemapper.SixAxis;
using DSRemapper.Types;
using System;
using System.CodeDom;
using System.Collections.Specialized;
using System.IO.Hashing;
using System.Runtime.InteropServices;
using static System.Net.Mime.MediaTypeNames;

namespace DSRemapper.DualShock
{
    /*
     * Dualshock vendor id: 054C
     * Dualshock product id: 09CC (of my dualshock at least)
     */
    public class DualShockInfo : IDSInputDeviceInfo
    {
        public DualShockInfo(string path, string name, string id, int vendorId, int productId)
            : base(path, name, id, vendorId, productId) {}

        public override IDSInputController CreateController()
        {
            return new DualShock(this);
        }

        public override string ToString()
        {
            return $"Device {Name} [{Id}] [{VendorId:X4}][{ProductId:X4}]";
        }

        /// <summary>
        /// Conversion from DSHidInfo to DualShockInfo to make both "structures" independent
        /// </summary>
        /// <param name="info">DSHidInfo</param>
        public static explicit operator DualShockInfo(DSHidInfo info) => new(info.Path,info.Name,info.Id,info.VendorId,info.ProductId);
        /// <summary>
        /// Conversion from DualShockInfo to DSHidInfo to make both "structures" independent
        /// </summary>
        /// <param name="info">DSHidInfo</param>
        public static explicit operator DSHidInfo(DualShockInfo info) => new(info.Path, info.Name, info.Id, info.VendorId, info.ProductId);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct ReportArray
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public byte[] data;

        public byte this[int index] { get => data[index]; }
    }

    [StructLayout(LayoutKind.Explicit, Size = 64)]
    internal struct DualShockInReport
    {
        [FieldOffset(0)]
        public byte id = 0;
        [FieldOffset(1)]
        public byte LX = 0;
        [FieldOffset(2)]
        public byte LY = 0;
        [FieldOffset(3)]
        public byte RX = 0;
        [FieldOffset(4)]
        public byte RY = 0;

        [FieldOffset(5)]
        private BitVector32 buttons = new BitVector32();
        private static BitVector32.Section[] masks = new BitVector32.Section[15];

        [FieldOffset(8)]
        public byte LT = 0;
        [FieldOffset(9)]
        public byte RT = 0;

        [FieldOffset(13)]
        public short GyroX = 0;
        [FieldOffset(15)]
        public short GyroY = 0;
        [FieldOffset(17)]
        public short GyroZ = 0;

        [FieldOffset(19)]
        public short AccelX = 0;
        [FieldOffset(21)]
        public short AccelY = 0;
        [FieldOffset(23)]
        public short AccelZ = 0;

        [FieldOffset(30)]
        private byte misc = 0;

        [FieldOffset(35)]
        private BitVector32 touchf1 = new();
        [FieldOffset(39)]
        private BitVector32 touchf2 = new();

        private static BitVector32.Section touchId = BitVector32.CreateSection(0x7F);
        private static BitVector32.Section touchPress = BitVector32.CreateSection(0x01, touchId);
        private static BitVector32.Section touchPosX = BitVector32.CreateSection(0xFFF, touchPress);
        private static BitVector32.Section touchPosY = BitVector32.CreateSection(0xFFF, touchPosX);

        public byte DPad => (byte)buttons[masks[0]];
        public bool Square => buttons[masks[1]] != 0;
        public bool Cross => buttons[masks[2]] != 0;
        public bool Circle => buttons[masks[3]] != 0;
        public bool Triangle => buttons[masks[4]] != 0;
        public bool L1 => buttons[masks[5]] != 0;
        public bool R1 => buttons[masks[6]] != 0;
        public bool L2 => buttons[masks[7]] != 0;
        public bool R2 => buttons[masks[8]] != 0;
        public bool Options => buttons[masks[9]] != 0;
        public bool Share => buttons[masks[10]] != 0;
        public bool L3 => buttons[masks[11]] != 0;
        public bool R3 => buttons[masks[12]] != 0;
        public bool PS => buttons[masks[13]] != 0;
        public bool TPad => buttons[masks[14]] != 0;

        public byte Baterry => (byte)(misc & 0x0F);
        public bool USB => (misc & (1 << 4)) != 0;

        public byte TF1Id => (byte)touchf1[touchId];
        public bool TF1Press => touchf1[touchPress] == 0;
        public short TF1PosX => (short)touchf1[touchPosX];
        public short TF1PosY => (short)touchf1[touchPosY];
        public byte TF2Id => (byte)touchf2[touchId];
        public bool TF2Press => touchf2[touchPress] == 0;
        public short TF2PosX => (short)touchf2[touchPosX];
        public short TF2PosY => (short)touchf2[touchPosY];

        static DualShockInReport(){
            masks[0] = BitVector32.CreateSection(0x0f);
            for(int i = 1; i < masks.Length; i++)
            {
                masks[i] = BitVector32.CreateSection(0x01, masks[i - 1]);
            }
        }

        public DualShockInReport() { }
    }

    public class DualShockScanner : IDSDeviceScanner
    {
        public IDSInputDeviceInfo[] ScanDevices() => WmiEnumerator
            .EnumerateDevices(0x054C).Select(i => (DualShockInfo)i).ToArray();

    }
    public class DualShock : IDSInputController
    {
        private readonly HidDevice hidDevice;

        private DSVector3 lastGyro = new();
        private CustomMotionProcess motPro = new();
        private ExpMovingAverageVector3 gyroAvg = new();

        private int offset = 0;
        DualShockInReport strRawReport;
        byte[] rawReport = Array.Empty<byte>(), crc = Array.Empty<byte>();
        DSInputReport report = new();
        List<byte> sendReport = new();

        public string Id => hidDevice.Information.Id;

        public string Name => "DualShock 4";

        public string Type => "DS";

        //bool _isConnected = false;

        public bool IsConnected => hidDevice.IsOpen;

        //public bool IsConnected { get => _isConnected; private set => _isConnected = value; }

        public DualShock(DualShockInfo info)
        {
            hidDevice = new((DSHidInfo)info);
            Logger.Log($"Device connected: {Name} [{Id}] [{hidDevice.Information.VendorId:X4}][{hidDevice.Information.ProductId:X4}]");
        }
        public void Connect()
        {
            hidDevice.OpenDevice(false);
            if (IsConnected)
            {
                rawReport = new byte[hidDevice.Capabilities.InputReportByteLength];
                GetFeatureReport();
            }
        }

        public void Disconnect()
        {
            hidDevice.CancelIO();
            hidDevice.CloseDevice();
        }

        public void Dispose()
        {
            Disconnect();
            hidDevice.Dispose();
        }

        public void ForceDisconnect()
        {

        }
        public void GetFeatureReport()
        {
            byte[] fetRep = new byte[64];
            fetRep[0] = 0x05;
            hidDevice.GetFeature(fetRep);

            DSOutputReport report = new();//Utils.CreateOutputReport();

            if (rawReport.Length > 64)
            {
                offset = 2;
                sendReport = new(new byte[79])
                {
                    [0] = 0xa2, // Output report header, needs to be included in crc32
                    [1] = 0x11, // Output report 0x11
                    [2] = 0xc0, //0xc0 HID + CRC according to hid-sony
                    [3] = 0x20, //0x20 ????
                    [4] = 0x07, // Set blink + leds + motor

                    // rumble
                    [7] = (byte)(report.Weak * 255),
                    [8] = (byte)(report.Strong * 255),
                    // colour
                    [9] = (byte)(report.Red * 255),
                    [10] = (byte)(report.Green * 255),
                    [11] = (byte)(report.Blue * 255),
                    // flash time
                    [12] = (byte)(report.OnTime * 255),
                    [13] = (byte)(report.OffTime * 255)
                };
            }
            else
            {
                sendReport = new(new byte[hidDevice.Capabilities.OutputReportByteLength])
                {
                    [0] = 0x05,
                    [1] = 0xff,

                    // rumble
                    [4] = (byte)(report.Weak * 255),
                    [5] = (byte)(report.Strong * 255),
                    // colour
                    [6] = (byte)(report.Red * 255),
                    [7] = (byte)(report.Green * 255),
                    [8] = (byte)(report.Blue * 255),
                    // flash time
                    [9] = (byte)(report.OnTime * 255),
                    [10] = (byte)(report.OffTime * 255)
                };
            }
        }

        public DSInputReport GetInputReport()
        {
            hidDevice.ReadFile(rawReport);
            GCHandle ptr = GCHandle.Alloc(rawReport, GCHandleType.Pinned);
            strRawReport = Marshal.PtrToStructure<DualShockInReport>(new IntPtr(ptr.AddrOfPinnedObject().ToInt64()+offset));
            ptr.Free();

            report.LX = AxisToFloat((sbyte)(strRawReport.LX - 128));
            report.LY = AxisToFloat((sbyte)(strRawReport.LY - 128));
            report.RX = AxisToFloat((sbyte)(strRawReport.RX - 128));
            report.RY = AxisToFloat((sbyte)(strRawReport.RY - 128));

            report.Povs[0].SetDSPov(strRawReport.DPad);

            report.Square = strRawReport.Square;
            report.Cross = strRawReport.Cross;
            report.Circle = strRawReport.Circle;
            report.Triangle = strRawReport.Triangle;

            report.L1 = strRawReport.L1;
            report.R1 = strRawReport.R1;
            report.L2 = strRawReport.L2;
            report.R2 = strRawReport.R2;
            report.Share = strRawReport.Share;
            report.Options = strRawReport.Options;
            report.L3 = strRawReport.L3;
            report.R3 = strRawReport.R3;

            report.PS = strRawReport.PS;
            report.TouchClick = strRawReport.TPad;

            report.LTrigger = AxisToFloat(strRawReport.LT);
            report.RTrigger = AxisToFloat(strRawReport.RT);
            report.Battery = Math.Clamp(strRawReport.Baterry / 10f, 0f, 1f);
            report.Usb = strRawReport.USB;

            report.SixAxis[1].X = -strRawReport.GyroX * (2000f / 32767f);
            report.SixAxis[1].Y = -strRawReport.GyroY * (2000f / 32767f);
            report.SixAxis[1].Z = strRawReport.GyroZ * (2000f / 32767f);
            report.SixAxis[0].X = -strRawReport.AccelX / 8192f;
            report.SixAxis[0].Y = -strRawReport.AccelY / 8192f;
            report.SixAxis[0].Z = strRawReport.AccelZ / 8192f;

            DSVector3 temp = (report.Gyro - lastGyro);
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

            report.TouchPadSize = new(1920, 943);

            report.Touch[0].Pressed = strRawReport.TF1Press;
            report.Touch[0].Id = strRawReport.TF1Id;
            report.Touch[0].Pos.X = strRawReport.TF1PosX;
            report.Touch[0].Pos.Y = strRawReport.TF1PosY;
            report.Touch[0].Pos /= report.TouchPadSize;

            report.Touch[1].Pressed = strRawReport.TF2Press;
            report.Touch[1].Id = strRawReport.TF2Id;
            report.Touch[1].Pos.X = strRawReport.TF2PosX;
            report.Touch[1].Pos.Y = strRawReport.TF2PosY;
            report.Touch[1].Pos /= report.TouchPadSize;

            return report;
        }

        public void SendOutputReport(DSOutputReport report)
        {
            if (offset > 0)
            {
                // rumble
                sendReport[7] = (byte)(report.Weak * 255);
                sendReport[8] = (byte)(report.Strong * 255);
                // colour
                sendReport[9] = (byte)(report.Red * 255);
                sendReport[10] = (byte)(report.Green * 255);
                sendReport[11] = (byte)(report.Blue * 255);
                // flash time
                sendReport[12] = (byte)(report.OnTime * 255);
                sendReport[13] = (byte)(report.OffTime * 255);

                crc = Crc32.Hash(sendReport.GetRange(0, 75).ToArray());
                sendReport[75] = crc[0];
                sendReport[76] = crc[1];
                sendReport[77] = crc[2];
                sendReport[78] = crc[3];

                hidDevice.WriteFile(sendReport.GetRange(1, 78).ToArray());
            }
            else
            {
                // rumble
                sendReport[4] = (byte)(report.Weak * 255);
                sendReport[5] = (byte)(report.Strong * 255);
                // colour
                sendReport[6] = (byte)(report.Red * 255);
                sendReport[7] = (byte)(report.Green * 255);
                sendReport[8] = (byte)(report.Blue * 255);
                // flash time
                sendReport[9] = (byte)(report.OnTime * 255);
                sendReport[10] = (byte)(report.OffTime * 255);

                hidDevice.WriteFile(sendReport.ToArray());
            }
        }
        private static float AxisToFloat(sbyte axis) => axis / (axis < 0 ? 128f : 127f);
        private static float AxisToFloat(byte axis) => axis / 255f;
    }
}