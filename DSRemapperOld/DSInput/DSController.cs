using System.Runtime.InteropServices;
using System.IO.Hashing;
using System.Numerics;
using DSRemapper.Remapper;
using DSRemapper.DSInput.HidCom;
using Windows.Devices.HumanInterfaceDevice;
using Windows.ApplicationModel.Appointments;

namespace DSRemapper.DSInput
{
    internal class DSController : IDSInputController
    {
        private readonly DSHidDevice hidDevice;
        private DSVector3 lastGyro = new();//gyroCal = new(0.18311106f, -0.48829615f, -0.12207404f)
        private CustomMotionProcess motPro = new();
        private ExpMovingAverageVector3 gyroAvg=new();//accelAvg=new()

        public string Id => hidDevice.readSerial();
        public bool IsConnected => hidDevice.IsOpen;
        public string ControllerName => "DualShock4";
        public ControllerType Type => ControllerType.DS;

        private int offset = 0;
        byte[] rawReport = Array.Empty<byte>(), crc = Array.Empty<byte>();
        DSInputReport report = new();
        List<byte> sendReport = new();

        public DSController(DSHidDevice device)
        {
            hidDevice = device;
            Console.WriteLine(device.Information.Path);
        }
        public void Dispose()
        {
            Disconnect();
            GC.SuppressFinalize(this);
        }

        ~DSController()
        {
            Disconnect();
        }
        public void Connect()
        {
            hidDevice.OpenDevice(false);
            if (IsConnected)
            {
                rawReport = new byte[hidDevice.Capabilities.InputReportByteLength];
                GetFeatureReport();
                //SendOutputReport(Utils.CreateOutputReport());
            }
        }
        public void Disconnect()
        {
            hidDevice.CancelIO();
            hidDevice.CloseDevice();
        }
        public void ForceDisconnect()
        {
            if (IsConnected)
            {
                byte[] disconnectReport = new byte[65];
                disconnectReport[0] = 0xE2;
                disconnectReport[1] = 0x02;
                Array.Clear(disconnectReport, 2, 63);

                hidDevice.SetFeature(disconnectReport);
            }
        }
        public void GetFeatureReport()
        {
            byte[] fetRep = new byte[64];
            fetRep[0] = 0x05;
            hidDevice.GetFeature(fetRep);

            DSOutputReport report = Utils.CreateOutputReport();

            if (rawReport.Length>64)
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

            report.LX = AxisToFloat((sbyte)(rawReport[offset + 1] - 128));
            report.LY = AxisToFloat((sbyte)(rawReport[offset + 2] - 128));
            report.RX = AxisToFloat((sbyte)(rawReport[offset + 3] - 128));
            report.RY = AxisToFloat((sbyte)(rawReport[offset + 4] - 128));

            int constOffset = offset + 5;
            byte dPad = (byte)(rawReport[constOffset] & 0x0F);
            report.Povs[0].SetDSPov(dPad);

            report.Square = Convert.ToBoolean(rawReport[constOffset] & (1 << 4));
            report.Cross = Convert.ToBoolean(rawReport[constOffset] & (1 << 5));
            report.Circle = Convert.ToBoolean(rawReport[constOffset] & (1 << 6));
            report.Triangle = Convert.ToBoolean(rawReport[constOffset] & (1 << 7));

            constOffset = offset + 6;
            report.L1 = Convert.ToBoolean(rawReport[constOffset] & (1 << 0));
            report.R1 = Convert.ToBoolean(rawReport[constOffset] & (1 << 1));
            report.L2 = Convert.ToBoolean(rawReport[constOffset] & (1 << 2));
            report.R2 = Convert.ToBoolean(rawReport[constOffset] & (1 << 3));
            report.Share = Convert.ToBoolean(rawReport[constOffset] & (1 << 4));
            report.Options = Convert.ToBoolean(rawReport[constOffset] & (1 << 5));
            report.L3 = Convert.ToBoolean(rawReport[constOffset] & (1 << 6));
            report.R3 = Convert.ToBoolean(rawReport[constOffset] & (1 << 7));

            constOffset = offset + 7;
            report.PS = Convert.ToBoolean(rawReport[constOffset] & (1 << 0));
            report.TouchClick = Convert.ToBoolean(rawReport[constOffset] & (1 << 1));

            report.LTrigger = AxisToFloat(rawReport[offset + 8]);
            report.RTrigger = AxisToFloat(rawReport[offset + 9]);
            report.Battery = Math.Clamp((rawReport[offset + 30] & 0xF) / 10f, 0f, 1f);
            report.Usb = Convert.ToBoolean(rawReport[offset + 30] & (1 << 4));

            report.SixAxis[1].X = -(short)((rawReport[offset + 14] << 8) | rawReport[offset + 13]) * (2000f / 32767f);
            report.SixAxis[1].Y = -(short)((rawReport[offset + 16] << 8) | rawReport[offset + 15]) * (2000f / 32767f);
            report.SixAxis[1].Z = (short)((rawReport[offset + 18] << 8) | rawReport[offset + 17]) * (2000f / 32767f);
            report.SixAxis[0].X = -(short)((rawReport[offset + 20] << 8) | rawReport[offset + 19]) / 8192f;
            report.SixAxis[0].Y = -(short)((rawReport[offset + 22] << 8) | rawReport[offset + 21]) / 8192f;
            report.SixAxis[0].Z = (short)((rawReport[offset + 24] << 8) | rawReport[offset + 23]) / 8192f;

            DSVector3 temp = (report.Gyro - lastGyro);
            if (temp.Length < 1f)
                gyroAvg.Update(report.Gyro,200);
            lastGyro = report.Gyro;

            report.Gyro -= gyroAvg.Mean;

            motPro.Update(report.RawAccel, report.Gyro);

            report.Grav = -motPro.grav;
            report.Accel = motPro.Accel;
            report.Rotation = motPro.rotation;
            report.DeltaRotation = motPro.deltaRotation;
            report.deltaTime = motPro.DeltaTime;

            report.TouchPadSize = new(1920, 943);

            report.Touch[0].Pressed = !Convert.ToBoolean(rawReport[offset + 35] & (1 << 7));
            report.Touch[0].Id = rawReport[offset + 35] & ~(1 << 7);
            report.Touch[0].Pos.X = rawReport[offset + 36] | ((rawReport[offset + 37] & 0x0F) << 8);
            report.Touch[0].Pos.Y = ((rawReport[offset + 37] & 0xF0) >> 4) | (rawReport[offset + 38] << 4);
            report.Touch[0].Pos /= report.TouchPadSize;

            report.Touch[1].Pressed = !Convert.ToBoolean(rawReport[offset + 39] & (1 << 7));
            report.Touch[1].Id = rawReport[offset + 39] & ~(1 << 7);
            report.Touch[1].Pos.X = rawReport[offset + 40] | ((rawReport[offset + 41] & 0x0F) << 8);
            report.Touch[1].Pos.Y = ((rawReport[offset + 41] & 0xF0) >> 4) | (rawReport[offset + 42] << 4);
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
