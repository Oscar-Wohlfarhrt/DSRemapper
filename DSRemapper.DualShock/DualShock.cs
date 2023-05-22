using DSRemapper.Core;
using DSRemapper.Types;

namespace DSRemapper.DualShock
{
    public class DualShockInfo : IDSInputDeviceInfo
    {
        public DualShockInfo(string id, string name, string description = "none")
            : base(id, name, description) { }

        public override IDSInputController CreateController()
        {
            return new DualShock();
        }
    }

    public class DualShockScanner : IDSDeviceScanner
    {
        public IDSInputDeviceInfo[] ScanDevices()
        {
            return new[]
            {
                new DualShockInfo("1","a"),
                new ("2","b"),
                new ("3","c"),
                new ("4","d"),
                new ("5","e"),
            };
        }
    }
    public class DualShock : IDSInputController
    {
        public string Id => throw new NotImplementedException();

        public string ControllerName => throw new NotImplementedException();

        public string Type => throw new NotImplementedException();

        public bool IsConnected => throw new NotImplementedException();

        public DualShock()
        {

        }
        public void Connect()
        {
            throw new NotImplementedException();
        }

        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void ForceDisconnect()
        {
            throw new NotImplementedException();
        }

        public DSInputReport GetInputReport()
        {
            throw new NotImplementedException();
        }

        public void SendOutputReport(DSOutputReport report)
        {
            throw new NotImplementedException();
        }
    }
}