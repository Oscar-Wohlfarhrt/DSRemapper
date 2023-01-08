using SharpDX.DirectInput;

using DSRemapper.DSInput.HidCom;
using System.IO.Ports;

namespace DSRemapper.DSInput
{
    public class DSTouch
    {
        public int Id { get; set; } = 0;
        public bool Pressed { get; set; } = false;
        public DSVector2 Pos { get; set; } = new();

        public override string ToString() => $"Id:{Id},P:{Pressed},{Pos}";
    }
    public struct DSInputReport
    {
        public float Battery { get; set; } = 0;
        public bool Usb { get; set; } = false;
        public float[] Axis { get; set; } = new float[6];
        public float[] Sliders { get; set; } = Array.Empty<float>();
        public bool[] Buttons { get; set; } = new bool[14];
        public DSPov[] Pov { get; set; } = new DSPov[1] { new() };
        public DSVector3[] SixAxis { get; set; } = new DSVector3[4] { new(), new(), new(), new() };
        public DSQuaternion DeltaRotation { get; set; } = new();
        public DSQuaternion Rotation { get; set; } = new();

        public DSTouch[] Touch { get; set; } = new DSTouch[2] { new(),new()};
        public DSVector2 TouchPadSize { get; set; } = new();

        public float deltaTime = 0;

        public DSInputReport() { }

        #region Axis
        public float LX { get { return Axis[0]; } set { Axis[0] = value; } }
        public float LY { get { return Axis[1]; } set { Axis[1] = value; } }
        public float RX { get { return Axis[2]; } set { Axis[2] = value; } }
        public float RY { get { return Axis[3]; } set { Axis[3] = value; } }
        public float LTrigger { get { return Axis[4]; } set { Axis[4] = value; } }
        public float RTrigger { get { return Axis[5]; } set { Axis[5] = value; } }
        #endregion Axis

        #region POV1
        public bool Up { get { return Pov[0].Up; } set { Pov[0].Up = value; } }
        public bool Right { get { return Pov[0].Right; } set { Pov[0].Right = value; } }
        public bool Down { get { return Pov[0].Down; } set { Pov[0].Down = value; } }
        public bool Left { get { return Pov[0].Left; } set { Pov[0].Left = value; } }
        #endregion POV1

        #region DS4Layout
        public bool Square { get { return Buttons[0]; } set { Buttons[0] = value; } }
        public bool Cross { get { return Buttons[1]; } set { Buttons[1] = value; } }
        public bool Circle { get { return Buttons[2]; } set { Buttons[2] = value; } }
        public bool Triangle { get { return Buttons[3]; } set { Buttons[3] = value; } }
        public bool L1 { get { return Buttons[4]; } set { Buttons[4] = value; } }
        public bool R1 { get { return Buttons[5]; } set { Buttons[5] = value; } }
        public bool L2 { get { return Buttons[6]; } set { Buttons[6] = value; } }
        public bool R2 { get { return Buttons[7]; } set { Buttons[7] = value; } }
        public bool Share { get { return Buttons[8]; } set { Buttons[8] = value; } }
        public bool Options { get { return Buttons[9]; } set { Buttons[9] = value; } }
        public bool L3 { get { return Buttons[10]; } set { Buttons[10] = value; } }
        public bool R3 { get { return Buttons[11]; } set { Buttons[11] = value; } }
        public bool PS { get { return Buttons[12]; } set { Buttons[12] = value; } }
        public bool TouchClick { get { return Buttons[13]; } set { Buttons[13] = value; } }
        #endregion DS4Layout

        #region XboxLayout
        public bool X { get { return Buttons[0]; } set { Buttons[0] = value; } }
        public bool A { get { return Buttons[1]; } set { Buttons[1] = value; } }
        public bool B { get { return Buttons[2]; } set { Buttons[2] = value; } }
        public bool Y { get { return Buttons[3]; } set { Buttons[3] = value; } }
        public bool LButton { get { return Buttons[4]; } set { Buttons[4] = value; } }
        public bool RButton { get { return Buttons[5]; } set { Buttons[5] = value; } }
        public bool Back { get { return Buttons[8]; } set { Buttons[8] = value; } }
        public bool Start { get { return Buttons[9]; } set { Buttons[9] = value; } }
        public bool LThumb { get { return Buttons[10]; } set { Buttons[10] = value; } }
        public bool RThumb { get { return Buttons[11]; } set { Buttons[11] = value; } }
        public bool Guide { get { return Buttons[12]; } set { Buttons[12] = value; } }
        #endregion XboxLayout

        #region SixAxis
        public DSVector3 RawAccel { get { return SixAxis[0]; } set { SixAxis[0] = value; } }
        public DSVector3 Gyro { get { return SixAxis[1]; } set { SixAxis[1] = value; } }
        public DSVector3 Grav { get { return SixAxis[2]; } set { SixAxis[2] = value; } }
        public DSVector3 Accel { get { return SixAxis[3]; } set { SixAxis[3] = value; } }
        #endregion SixAxis
    }
    public struct DSOutputReport
    {
        public float[] rumble = new float[2];
        public DSLight Led { get; set; } = new();
        public float[] OnOff { get; set; } = new float[2];

        public DSOutputReport() { }

        #region DS4Layout
        public float Right { get { return rumble[0]; } set { rumble[0] = value; } }
        public float Left { get { return rumble[1]; } set { rumble[1] = value; } }
        public float Weak { get { return rumble[0]; } set { rumble[0] = value; } }
        public float Strong { get { return rumble[1]; } set { rumble[1] = value; } }
        public float Red { get { return Led.Red; } set { Led.Red = value; } }
        public float Green { get { return Led.Green; } set { Led.Green = value; } }
        public float Blue { get { return Led.Blue; } set { Led.Blue = value; } }
        public float OnTime { get { return Led.OnTime; } set { Led.OnTime = value; } }
        public float OffTime { get { return Led.OffTime; } set { Led.OffTime = value; } }
        #endregion DS4Layout

    }
    public enum ControllerType
    {
        Unkown,
        DS,
        DI,
        COM,
        TCP,
    }

    public interface IDSInputController : IDisposable
    {
        public string Id { get; }
        public string ControllerName { get; }
        public ControllerType Type { get; }
        public bool IsConnected { get; }
        public void Connect();
        public void Disconnect();
        public DSInputReport GetInputReport();
        public void SendOutputReport(DSOutputReport report);
    }

    public static class DSInputControllers
    {
        private static readonly ushort[] vendorBlackList = new ushort[] { 0x054C/*, 0x045E*/ };
        private static readonly ushort[] productBlackList = new ushort[] { 0x05C4 };
        private static List<DIDeviceInfo> tempDevInfo = new();
        private static int lastCount=0;
        public static bool RefreshDevices()
        {
            List<DIDeviceInfo> xDevices = DIController.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly).ToList();
            string[] comDevices = System.IO.Ports.SerialPort.GetPortNames();
            if ((xDevices.Count + comDevices.Length) != lastCount)
            {
                lastCount = xDevices.Count + comDevices.Length;
                tempDevInfo = xDevices;
                return true;
            }

            return false;
        }

        public static IEnumerable<IDSInputController> GetDevices()
        {
            List<IDSInputController> controllers = new();

            foreach (var info in tempDevInfo)
            {
                Console.WriteLine($"Name {info.ProductName} | Ids: {info.ProductId:X4}-{info.VendorId:X4}");
                if (!vendorBlackList.Contains(info.VendorId) && !(info.VendorId == 0x045E && info.ProductId == 0x0280))
                {
                    controllers.Add(new DIController(info));
                }
            }
            foreach (var dev in DSHidDevice.Enumerate(0x054C))
            {
                if (!productBlackList.Contains((ushort)dev.Attributes.ProductId))
                {
                    controllers.Add(new DSController(dev));
                }
            }
            string[] ports = System.IO.Ports.SerialPort.GetPortNames();

            Console.WriteLine("=======COMM=======");
            foreach (var com in ports)
            {
                Console.WriteLine(com);
                controllers.Add(new COMController(com));
            }

            return controllers;
        }
    }
}
