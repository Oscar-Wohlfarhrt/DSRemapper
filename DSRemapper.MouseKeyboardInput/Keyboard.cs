using Vortice.DirectInput;
using DSRemapper.Core;
using static DSRemapper.MouseKeyboardInput.KeyboardScanner;
using DSRemapper.Types;

namespace DSRemapper.MouseKeyboardInput
{
    public class KeyboardInfo : IDSInputDeviceInfo
    {
        public DeviceInstance DeviceInstance { get; private set; }
        public Guid ProductGuid { get { return DeviceInstance.ProductGuid; } }
        public Guid InstanceGuid { get { return DeviceInstance.InstanceGuid; } }

        private byte[] ProductBytes { get { return ProductGuid.ToByteArray(); } }

        public string Id => InstanceGuid.ToString();
        public string Name => DeviceInstance.ProductName;
        public int ProductId { get { return BitConverter.ToUInt16(ProductBytes, 2); } }
        public int VendorId { get { return BitConverter.ToUInt16(ProductBytes, 0); } }

        public KeyboardInfo(DeviceInstance deviceInstance)
        {
            DeviceInstance = deviceInstance;
        }

        public IDSInputController CreateController()
        {
            return new Keyboard(this);
        }
    }
    public class KeyboardScanner : IDSDeviceScanner
    {
        public static IDirectInput8 DI = DInput.DirectInput8Create();
        public IDSInputDeviceInfo[] ScanDevices()
        {
            return DI.GetDevices(DeviceClass.Keyboard, DeviceEnumerationFlags.AttachedOnly)
                .Select( devInfo => new KeyboardInfo(devInfo)).ToArray();
        }
    }

    public class Keyboard : IDSInputController
    {
        private readonly KeyboardInfo deviceInfo;
        private readonly IDirectInputDevice8 device;
        private readonly DSInputReport report = new();

        public string Id => deviceInfo.Id;

        public string Name => deviceInfo.Name;

        public string Type => "Keyboard";

        public bool IsConnected { get; private set; }

        public Keyboard(KeyboardInfo info)
        {
            deviceInfo = info;
            device = DI.CreateDevice(info.InstanceGuid);
            report = new(0, 0, 255, 0, 0);
        }

        public void Connect()
        {
            device.SetCooperativeLevel(IntPtr.Zero, CooperativeLevel.NonExclusive | CooperativeLevel.Foreground);
            device.Properties.BufferSize = 16;
            IsConnected = device.SetDataFormat<RawKeyboardState>().Success;
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
                        KeyboardState state= device.GetCurrentKeyboardState();
                        report.SetButtons(state.AllKeys.Select(state.IsPressed).ToArray());
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