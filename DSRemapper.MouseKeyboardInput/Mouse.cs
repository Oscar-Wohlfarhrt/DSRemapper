using Vortice.DirectInput;
using DSRemapper.Core;
using static DSRemapper.MouseKeyboardInput.KeyboardScanner;
using DSRemapper.Types;

namespace DSRemapper.MouseKeyboardInput
{
    public class MouseInfo : IDSInputDeviceInfo
    {
        public DeviceInstance DeviceInstance { get; private set; }
        public Guid ProductGuid { get { return DeviceInstance.ProductGuid; } }
        public Guid InstanceGuid { get { return DeviceInstance.InstanceGuid; } }

        private byte[] ProductBytes { get { return ProductGuid.ToByteArray(); } }

        public string Id => InstanceGuid.ToString();
        public string Name => DeviceInstance.ProductName;
        public int ProductId { get { return BitConverter.ToUInt16(ProductBytes, 2); } }
        public int VendorId { get { return BitConverter.ToUInt16(ProductBytes, 0); } }

        public MouseInfo(DeviceInstance deviceInstance)
        {
            DeviceInstance = deviceInstance;
        }

        public IDSInputController CreateController()
        {
            return new Mouse(this);
        }
    }
    public class MouseScanner : IDSDeviceScanner
    {
        public IDSInputDeviceInfo[] ScanDevices()
        {
            return DI.GetDevices(DeviceClass.Pointer, DeviceEnumerationFlags.AttachedOnly)
                .Select( devInfo => new MouseInfo(devInfo)).ToArray();
        }
    }

    public class Mouse : IDSInputController
    {
        private readonly MouseInfo deviceInfo;
        private readonly IDirectInputDevice8 device;
        private readonly DSInputReport report = new();

        public string Id => deviceInfo.Id;

        public string Name => deviceInfo.Name;

        public string Type => "Mouse";

        public bool IsConnected { get; private set; }

        public Mouse(MouseInfo info)
        {
            deviceInfo = info;
            device = DI.CreateDevice(info.InstanceGuid);
            report = new(3, 0, 8, 0, 0);
        }

        public void Connect()
        {
            device.SetCooperativeLevel(IntPtr.Zero, CooperativeLevel.NonExclusive | CooperativeLevel.Foreground);
            device.Properties.BufferSize = 16;
            IsConnected = device.SetDataFormat<RawMouseState>().Success;
            device.Acquire();
            if (!IsConnected)
                Disconnect();
        }

        public void Disconnect()
        {
            IsConnected = false;
            device.Unacquire();
        }


        public void Dispose()
        {
            Disconnect();
        }

        public DSInputReport GetInputReport()
        {
            try
            {
                if (IsConnected)
                {
                    var res = device.Poll();
                    if (res.Success)
                    {
                        MouseState state = device.GetCurrentMouseState();
                        report.Axis[0] = state.X / 120f;
                        report.Axis[1] = state.Y / 120f;
                        report.Axis[2] = state.Z / 120f;
                        report.SetButtons(state.Buttons);
                    }
                    else
                        device.Acquire();
                }
            }
            catch { }

            return report;
        }

        public void SendOutputReport(DSOutputReport report)
        {

        }
    }
}