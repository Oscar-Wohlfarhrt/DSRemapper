using DSRemapper.Core;
using DSRemapper.Types;

namespace DSRemapper.DualShock
{
    public class DualShockScanner : IDSDeviceScanner<DualShock>
    {
        public DSInputDeviceInfo<DualShock>[] ScanDevices()
        {
            return new[]
            {
                new DSInputDeviceInfo<DualShock>("1","a"),
                new DSInputDeviceInfo<DualShock>("2","b"),
                new DSInputDeviceInfo<DualShock>("3","c"),
                new DSInputDeviceInfo<DualShock>("4","d"),
                new DSInputDeviceInfo<DualShock>("5","e"),
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