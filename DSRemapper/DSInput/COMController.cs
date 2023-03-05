﻿using System.Runtime.InteropServices;
using System.IO.Ports;
using Microsoft.VisualBasic;
using DSRemapper.DSOutput;

namespace DSRemapper.DSInput
{
    internal class COMController : IDSInputController
    {
        const int BaudRate = 57600;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct COMInfoReport
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
        struct COMInputReport
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
        struct COMOutputReport
        {
            public readonly byte code = 2;
            public byte id = 0;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public short[] Motors = new short[6];
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public byte[] Leds = new byte[6];

            public COMOutputReport() { }
        };
        private static int COMInfoReportSize { get => Marshal.SizeOf(typeof(COMInfoReport)); }
        private static int COMInputReportSize { get => Marshal.SizeOf(typeof(COMInputReport)); }
        private static int COMOutputReportSize { get => Marshal.SizeOf(typeof(COMOutputReport)); }

        private static readonly byte[] infoReportRequst = new byte[] { 0x00 };
        private static readonly byte[] inputReportRequst = new byte[] { 0x01 };

        private readonly string id;
        public string Id => id;
        public string ControllerName => $"Controller {id}";
        public ControllerType Type => ControllerType.COM;
        public bool IsConnected => isConnected;
        private bool isConnected = false;

        private readonly SerialPort sp;
        private COMInputReport rawReport=new();
        private DSInputReport report=new();
        private COMInfoReport? information;

        private CustomMotionProcess motPro = new();
        private ExpMovingAverageVector3 gyroAvg = new();
        private DSVector3 lastGyro = new();

        private const int readTimeout = 500;

        public COMController(string port)
        {
            id = port;
            report = new(sliders:6,buttons:32,povs:2);
            sp = new(port, BaudRate)
            {
                ReadTimeout = readTimeout
            };
        }

        public void Connect()
        {
            if (!sp.IsOpen)
            {
                sp.Open();
                isConnected = true;
            }
        }
        public void Disconnect()
        {
            if (sp.IsOpen)
            {
                sp.Close();
                isConnected = false;
            }
        }
        public void ForceDisconnect()
        {

        }
        public void Dispose()
        {
            Disconnect();
            sp.Dispose();
        }

        private bool Read(byte[] buffer,int offset,int count)
        {
            DateTime timeout = DateTime.Now.AddMilliseconds(readTimeout);
            while (sp.BytesToRead < count && DateTime.Now < timeout) ;
            if (DateTime.Now < timeout)
            {
                sp.BaseStream.Read(buffer, offset, count);
                return true;
            }
            return false;
        }

        private COMInfoReport? ReadInfoReport()
        {
            if (sp.BytesToRead > 0)
                sp.ReadExisting();

            COMInfoReport? report = null;
            sp.Write(infoReportRequst, 0, infoReportRequst.Length);
            byte[] buffer = new byte[COMInfoReportSize];
            if (Read(buffer, 0, buffer.Length))
            {
                GCHandle ptr = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                report = Marshal.PtrToStructure<COMInfoReport>(ptr.AddrOfPinnedObject());
                ptr.Free();
            }

            return report;
        }

        public DSInputReport GetInputReport()
        {
            if(sp.BytesToRead>0)
                sp.ReadExisting();

            information ??= ReadInfoReport();

            sp.Write(inputReportRequst,0, inputReportRequst.Length);
            byte[] buffer = new byte[COMInputReportSize];
            /*int bytesReaded = 0;
            while (bytesReaded < bufSize)
            {
                bytesReaded+=sp.Read(buffer, bytesReaded, bufSize-bytesReaded);
            }*/
            if (Read(buffer,0,buffer.Length))
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

                report.Buttons[0] = Convert.ToBoolean(rawReport.Buttons[0] & (1 << 0));
                report.Buttons[1] = Convert.ToBoolean(rawReport.Buttons[0] & (1 << 1));
                report.Buttons[2] = Convert.ToBoolean(rawReport.Buttons[0] & (1 << 2));
                report.Buttons[3] = Convert.ToBoolean(rawReport.Buttons[0] & (1 << 3));
                report.Buttons[4] = Convert.ToBoolean(rawReport.Buttons[0] & (1 << 4));
                report.Buttons[5] = Convert.ToBoolean(rawReport.Buttons[0] & (1 << 5));
                report.Buttons[6] = Convert.ToBoolean(rawReport.Buttons[0] & (1 << 6));
                report.Buttons[7] = Convert.ToBoolean(rawReport.Buttons[0] & (1 << 7));

                report.Buttons[8] = Convert.ToBoolean(rawReport.Buttons[1] & (1 << 0));
                report.Buttons[9] = Convert.ToBoolean(rawReport.Buttons[1] & (1 << 1));
                report.Buttons[10] = Convert.ToBoolean(rawReport.Buttons[1] & (1 << 2));
                report.Buttons[11] = Convert.ToBoolean(rawReport.Buttons[1] & (1 << 3));
                report.Buttons[12] = Convert.ToBoolean(rawReport.Buttons[1] & (1 << 4));
                report.Buttons[13] = Convert.ToBoolean(rawReport.Buttons[1] & (1 << 5));
                report.Buttons[14] = Convert.ToBoolean(rawReport.Buttons[1] & (1 << 6));
                report.Buttons[15] = Convert.ToBoolean(rawReport.Buttons[1] & (1 << 7));

                report.Buttons[16] = Convert.ToBoolean(rawReport.Buttons[2] & (1 << 0));
                report.Buttons[17] = Convert.ToBoolean(rawReport.Buttons[2] & (1 << 1));
                report.Buttons[18] = Convert.ToBoolean(rawReport.Buttons[2] & (1 << 2));
                report.Buttons[19] = Convert.ToBoolean(rawReport.Buttons[2] & (1 << 3));
                report.Buttons[20] = Convert.ToBoolean(rawReport.Buttons[2] & (1 << 4));
                report.Buttons[21] = Convert.ToBoolean(rawReport.Buttons[2] & (1 << 5));
                report.Buttons[22] = Convert.ToBoolean(rawReport.Buttons[2] & (1 << 6));
                report.Buttons[23] = Convert.ToBoolean(rawReport.Buttons[2] & (1 << 7));

                report.Buttons[24] = Convert.ToBoolean(rawReport.Buttons[3] & (1 << 0));
                report.Buttons[25] = Convert.ToBoolean(rawReport.Buttons[3] & (1 << 1));
                report.Buttons[26] = Convert.ToBoolean(rawReport.Buttons[3] & (1 << 2));
                report.Buttons[27] = Convert.ToBoolean(rawReport.Buttons[3] & (1 << 3));
                report.Buttons[28] = Convert.ToBoolean(rawReport.Buttons[3] & (1 << 4));
                report.Buttons[29] = Convert.ToBoolean(rawReport.Buttons[3] & (1 << 5));
                report.Buttons[30] = Convert.ToBoolean(rawReport.Buttons[3] & (1 << 6));
                report.Buttons[31] = Convert.ToBoolean(rawReport.Buttons[3] & (1 << 7));

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

            for(int i = 0; i < report.Rumble.Length;i++)
                outReport.Motors[i] = report.Rumble[i].ToShortAxis();
            for (int i = 0; i < report.ExtLeds.Length; i++)
                outReport.Leds[i] = (byte)report.ExtLeds[i];

            byte[] buffer = new byte[COMOutputReportSize];

            GCHandle ptr = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            Marshal.StructureToPtr(outReport, ptr.AddrOfPinnedObject(),false);
            ptr.Free();

            sp.Write(buffer, 0, buffer.Length);

            Read(buffer, 0, 1);
        }
        private static float AxisToFloat(int axis) => (float)axis / (short.MaxValue + (axis < 0 ? 1 : 0));
    }
}
