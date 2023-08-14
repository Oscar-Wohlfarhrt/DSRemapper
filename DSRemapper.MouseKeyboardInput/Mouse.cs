using Vortice.DirectInput;
using DSRemapper.Core;
using static DSRemapper.MouseKeyboardInput.KeyboardScanner;
using DSRemapper.Types;

namespace DSRemapper.MouseKeyboardInput
{
    /// <summary>
    /// Direct Input mouse information class
    /// </summary>
    public class MouseInfo : IDSInputDeviceInfo
    {
        /// <summary>
        /// DirectX DirectInput Device instance class used to create the mouse
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
        public MouseInfo(DeviceInstance deviceInstance)
        {
            DeviceInstance = deviceInstance;
        }
        /// <inheritdoc/>
        public IDSInputController CreateController()
        {
            return new Mouse(this);
        }
    }
    /// <summary>
    /// Direct input mouse scanner class
    /// </summary>
    public class MouseScanner : IDSDeviceScanner
    {
        /// <inheritdoc/>
        public IDSInputDeviceInfo[] ScanDevices()
        {
            return DI.GetDevices(DeviceClass.Pointer, DeviceEnumerationFlags.AttachedOnly)
                .Select( devInfo => new MouseInfo(devInfo)).ToArray();
        }
    }
    /// <summary>
    /// Direct Input Mouse class
    /// </summary>
    public class Mouse : IDSInputController
    {
        private readonly MouseInfo deviceInfo;
        private readonly IDirectInputDevice8 device;
        private readonly DSInputReport report = new();

        /// <inheritdoc/>
        public string Id => deviceInfo.Id;
        /// <inheritdoc/>
        public string Name => deviceInfo.Name;
        /// <inheritdoc/>
        public string Type => "Mouse";
        /// <inheritdoc/>
        public bool IsConnected { get; private set; }
        /// <summary>
        /// DirectInput Mouse class constructor
        /// </summary>
        /// <param name="info">DirectInput mouse information requiered to connect the mouse</param>
        public Mouse(MouseInfo info)
        {
            deviceInfo = info;
            device = DI.CreateDevice(info.InstanceGuid);
            report = new(3, 0, 8, 0, 0);
        }
        /// <inheritdoc/>
        public void Connect()
        {
            device.SetCooperativeLevel(IntPtr.Zero, CooperativeLevel.NonExclusive | CooperativeLevel.Foreground);
            device.Properties.BufferSize = 16;
            IsConnected = device.SetDataFormat<RawMouseState>().Success;
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
                        MouseState state = device.GetCurrentMouseState();
                        report.Axes[0] = state.X / 120f;
                        report.Axes[1] = state.Y / 120f;
                        report.Axes[2] = state.Z / 120f;
                        report.SetButtons(state.Buttons);
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