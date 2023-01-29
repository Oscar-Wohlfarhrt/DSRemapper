using SharpDX.DirectInput;

using DSRemapper.DSInput.HidCom;
using System.IO.Ports;

namespace DSRemapper.DSInput
{
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
            string[] comDevices = SerialPort.GetPortNames();
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
                if (!vendorBlackList.Contains(info.VendorId) &&
                    !(info.VendorId == 0x045E && info.ProductId == 0x0280) /*&&
                    !(info.VendorId == 0x1234 && info.ProductId == 0xBEAD)*/)
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
            string[] ports = SerialPort.GetPortNames().Distinct().ToArray();

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
