using Vortice.DirectInput;
using DSRemapper.Core;
using static DSRemapper.MouseKeyboardInput.KeyboardScanner;
using DSRemapper.Types;

namespace DSRemapper.MouseKeyboardInput
{
    /// <summary>
    /// Direct Input keyboard information class
    /// </summary>
    public class KeyboardInfo : IDSInputDeviceInfo
    {
        /// <summary>
        /// DirectX DirectInput Device instance class used to create the keyboard
        /// </summary>
        public DeviceInstance DeviceInstance { get; private set; }
        /// <summary>
        /// Gets product GUID of the device
        /// </summary>
        public Guid ProductGuid { get { return DeviceInstance.ProductGuid; } }
        /// <summary>
        /// Gets instance GUID of the device
        /// </summary>
        public Guid InstanceGuid { get { return DeviceInstance.InstanceGuid; } }

        private byte[] ProductBytes { get { return ProductGuid.ToByteArray(); } }
        /// <inheritdoc/>
        public string Id => InstanceGuid.ToString();
        /// <inheritdoc/>
        public string Name => DeviceInstance.ProductName;
        /// <summary>
        /// Gets device product id
        /// </summary>
        public int ProductId { get { return BitConverter.ToUInt16(ProductBytes, 2); } }
        /// <summary>
        /// Gets device vendor id
        /// </summary>
        public int VendorId { get { return BitConverter.ToUInt16(ProductBytes, 0); } }
        /// <summary>
        /// DirectInput Keyboard Info class contructor
        /// </summary>
        public KeyboardInfo(DeviceInstance deviceInstance)
        {
            DeviceInstance = deviceInstance;
        }
        /// <inheritdoc/>
        public IDSInputController CreateController()
        {
            return new Keyboard(this);
        }
    }
    /// <summary>
    /// Direct input keyboard scanner class
    /// </summary>
    public class KeyboardScanner : IDSDeviceScanner
    {
        /// <summary>
        /// DirecInput object used in the plugin
        /// </summary>
        internal static IDirectInput8 DI = DInput.DirectInput8Create();
        /// <inheritdoc/>
        public IDSInputDeviceInfo[] ScanDevices()
        {
            return DI.GetDevices(DeviceClass.Keyboard, DeviceEnumerationFlags.AttachedOnly)
                .Select( devInfo => new KeyboardInfo(devInfo)).ToArray();
        }
    }
    /// <summary>
    /// Direct Input Keyboard class
    /// </summary>
    public class Keyboard : IDSInputController
    {
        private readonly KeyboardInfo deviceInfo;
        private readonly IDirectInputDevice8 device;
        private readonly DSInputReport report = new();

        /// <inheritdoc/>
        public string Id => deviceInfo.Id;
        /// <inheritdoc/>
        public string Name => deviceInfo.Name;
        /// <inheritdoc/>
        public string Type => "Keyboard";
        /// <inheritdoc/>
        public bool IsConnected { get; private set; }
        /// <summary>
        /// DirectInput Keyboard class constructor
        /// </summary>
        /// <param name="info">DirectInput keyboard information requiered to connect the keyboard</param>
        public Keyboard(KeyboardInfo info)
        {
            deviceInfo = info;
            device = DI.CreateDevice(info.InstanceGuid);
            report = new(0, 0, 255, 0, 0);
        }
        /// <inheritdoc/>
        public void Connect()
        {
            device.SetCooperativeLevel(IntPtr.Zero, CooperativeLevel.NonExclusive | CooperativeLevel.Foreground);
            device.Properties.BufferSize = 16;
            IsConnected = device.SetDataFormat<RawKeyboardState>().Success;
            device.Acquire();
            if (!IsConnected)
                Disconnect();
        }
        /// <inheritdoc/>
        public void Disconnect()
        {
            IsConnected = false;
            device.Unacquire();
        }
        /// <inheritdoc/>
        public void Dispose()
        {
            Disconnect();
        }
        /// <inheritdoc/>
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
        /// <inheritdoc/>
        public void SendOutputReport(DSOutputReport report)
        {

        }
    }
}