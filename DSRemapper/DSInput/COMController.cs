using System.Runtime.InteropServices;
using System.IO.Ports;

namespace DSRemapper.DSInput
{
    internal class COMController : IDSInputController
    {
        const int BaudRate = 57600;
        const int bufSize = 18;

        struct OutputReport
        {
            [MarshalAs(UnmanagedType.ByValArray,SizeConst = 6)]
            public short[] Axis=new short[6];
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] Buttons=new byte[4];
            public short Pov=-1;

            public OutputReport() { }
        }
        private readonly string id;
        public string Id => id;
        public string ControllerName => $"Controller {id}";
        public ControllerType Type => ControllerType.COM;
        public bool IsConnected => isConnected;
        private bool isConnected = false;

        private System.IO.Ports.SerialPort sp;
        OutputReport serialReport=new();
        DSInputReport report=new();

        public COMController(string port)
        {
            id =port;
            report.Buttons = new bool[32];
            sp = new(port, BaudRate)
            {
                ReadTimeout = 100
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
        public void Dispose()
        {
            Disconnect();
            sp.Dispose();
        }
        public DSInputReport GetInputReport()
        {
            if(sp.BytesToRead>0)
                sp.ReadExisting();

            sp.Write("s");
            byte[] buffer = new byte[bufSize];
            int bytesReaded = 0;
            while (bytesReaded < bufSize)
            {
                bytesReaded+=sp.Read(buffer, bytesReaded, bufSize-bytesReaded);
            }
            GCHandle ptr = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            serialReport = Marshal.PtrToStructure<OutputReport>(ptr.AddrOfPinnedObject());
            ptr.Free();

            report.Axis[0] = AxisToFloat(serialReport.Axis[0]);
            report.Axis[1] = AxisToFloat(serialReport.Axis[1]);
            report.Axis[2] = AxisToFloat(serialReport.Axis[2]);
            report.Axis[3] = AxisToFloat(serialReport.Axis[3]);
            report.Axis[4] = AxisToFloat(serialReport.Axis[4]);

            report.Buttons[0] = Convert.ToBoolean(serialReport.Buttons[0] & (1 << 0));
            report.Buttons[1] = Convert.ToBoolean(serialReport.Buttons[0] & (1 << 1));
            report.Buttons[2] = Convert.ToBoolean(serialReport.Buttons[0] & (1 << 2));
            report.Buttons[3] = Convert.ToBoolean(serialReport.Buttons[0] & (1 << 3));
            report.Buttons[4] = Convert.ToBoolean(serialReport.Buttons[0] & (1 << 4));
            report.Buttons[5] = Convert.ToBoolean(serialReport.Buttons[0] & (1 << 5));
            report.Buttons[6] = Convert.ToBoolean(serialReport.Buttons[0] & (1 << 6));
            report.Buttons[7] = Convert.ToBoolean(serialReport.Buttons[0] & (1 << 7));

            report.Buttons[8] = Convert.ToBoolean(serialReport.Buttons[1] & (1 << 0));
            report.Buttons[9] = Convert.ToBoolean(serialReport.Buttons[1] & (1 << 1));
            report.Buttons[10] = Convert.ToBoolean(serialReport.Buttons[1] & (1 << 2));
            report.Buttons[11] = Convert.ToBoolean(serialReport.Buttons[1] & (1 << 3));
            report.Buttons[12] = Convert.ToBoolean(serialReport.Buttons[1] & (1 << 4));
            report.Buttons[13] = Convert.ToBoolean(serialReport.Buttons[1] & (1 << 5));
            report.Buttons[14] = Convert.ToBoolean(serialReport.Buttons[1] & (1 << 6));
            report.Buttons[15] = Convert.ToBoolean(serialReport.Buttons[1] & (1 << 7));

            report.Buttons[16] = Convert.ToBoolean(serialReport.Buttons[2] & (1 << 0));
            report.Buttons[17] = Convert.ToBoolean(serialReport.Buttons[2] & (1 << 1));
            report.Buttons[18] = Convert.ToBoolean(serialReport.Buttons[2] & (1 << 2));
            report.Buttons[19] = Convert.ToBoolean(serialReport.Buttons[2] & (1 << 3));
            report.Buttons[20] = Convert.ToBoolean(serialReport.Buttons[2] & (1 << 4));
            report.Buttons[21] = Convert.ToBoolean(serialReport.Buttons[2] & (1 << 5));
            report.Buttons[22] = Convert.ToBoolean(serialReport.Buttons[2] & (1 << 6));
            report.Buttons[23] = Convert.ToBoolean(serialReport.Buttons[2] & (1 << 7));

            report.Pov[0].Angle = serialReport.Pov;
            report.Pov[0].CalculateButtons();

            return report;
        }

        public void SendOutputReport(DSOutputReport report)
        {

        }
        private static float AxisToFloat(int axis) => (float)axis / (short.MaxValue + (axis < 0 ? 1 : 0));
    }
}
