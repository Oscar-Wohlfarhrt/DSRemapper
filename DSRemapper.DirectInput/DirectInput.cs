using DSRemapper.Core;
using DSRemapper.Types;

namespace DSRemapper.DirectInput
{
    public class DIScanner : IDSDeviceScanner
    {
        public IDSInputDeviceInfo[] ScanDevices()
        {
            return Array.Empty<IDSInputDeviceInfo>();
        }
    }
    public class DIDeviceInfo : IDSInputDeviceInfo
    {
        public DIDeviceInfo(string path, string name, string id, int vendorId, int productId)
            : base(path, name, id, vendorId, productId) { }

        public override IDSInputController CreateController()
        {
            return new DirectInput();
        }
    }
    public class DirectInput : IDSInputController
    {
        public string Id => "none";

        public string Name => "Unknow";

        public string Type => "DI";

        public bool IsConnected => false;

        public void Connect()
        {

        }

        public void Disconnect()
        {

        }

        public void Dispose()
        {

        }

        public void ForceDisconnect()
        {

        }

        public DSInputReport GetInputReport()
        {
            return new DSInputReport();
        }

        public void SendOutputReport(DSOutputReport report)
        {
            
        }
    }
}