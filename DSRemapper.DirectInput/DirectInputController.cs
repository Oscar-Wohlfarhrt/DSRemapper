using DSRemapper.Core;
using DSRemapper.DSLogger;
using DSRemapper.DSMath;
using DSRemapper.Types;
using SharpDX.DirectInput;

namespace DSRemapper.DirectInput
{
    public class DIScanner : IDSDeviceScanner
    {
        public static readonly SharpDX.DirectInput.DirectInput di = new();
        public IDSInputDeviceInfo[] ScanDevices()
        {
            return di.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly)
                .Select((x) => { return new DIDeviceInfo(x); }).ToArray();
        }
    }
    public class DIDeviceInfo : IDSInputDeviceInfo
    {

        public DeviceInstance DeviceInstance { get; private set; }
        public Guid ProductGuid { get { return DeviceInstance.ProductGuid; } }
        public Guid InstanceGuid { get { return DeviceInstance.InstanceGuid; } }

        private byte[] ProductBytes { get { return ProductGuid.ToByteArray(); } }

        public string Id => InstanceGuid.ToString();
        public string Name { get { return DeviceInstance.ProductName; } }
        public int ProductId { get { return BitConverter.ToUInt16(ProductBytes, 2); } }
        public int VendorId { get { return BitConverter.ToUInt16(ProductBytes, 0); } }


        public DIDeviceInfo(DeviceInstance deviceInstance)
        {
            DeviceInstance = deviceInstance;
        }

        public IDSInputController CreateController()
        {
            return new DirectInputController(this);
        }
    }
    public class DirectInputController : IDSInputController
    {
        static SharpDX.DirectInput.DirectInput DI => DIScanner.di;

        private readonly DIDeviceInfo deviceInfo;
        private readonly Joystick joy;
        private readonly DSInputReport report;

        public string Id => deviceInfo.Id;

        public string Name => deviceInfo.Name;

        public string Type => "DI";

        public bool IsConnected { get; private set; }

        public DirectInputController(DIDeviceInfo deviceInfo)
        {
            this.deviceInfo = deviceInfo;
            joy = new Joystick(DI, deviceInfo.InstanceGuid);

            report = new(6, 8, 32, 4, 0);
        }

        public void Connect()
        {
            joy.Acquire();
            IsConnected = true;
        }

        public void Disconnect()
        {
            IsConnected = false;
            joy.Unacquire();
        }

        public void Dispose()
        {
            Disconnect();
        }

        public DSInputReport GetInputReport()
        {
            try
            {
                if (!joy.IsDisposed && IsConnected)
                {
                    joy.Poll();
                    JoystickState state = joy.GetCurrentState();

                    for (int i = 0; i < report.Buttons.Length; i++)
                    {
                        report.Buttons[i] = state.Buttons[i];
                    }

                    report.Axes[0] = AxisToFloat(state.X - short.MaxValue);
                    report.Axes[1] = AxisToFloat(state.Y - short.MaxValue);
                    report.Axes[2] = AxisToFloat(state.Z - short.MaxValue);
                    report.Axes[3] = AxisToFloat(state.RotationZ - short.MaxValue);
                    report.Axes[4] = AxisToFloat(state.RotationX - short.MaxValue);
                    report.Axes[5] = AxisToFloat(state.RotationY - short.MaxValue);

                    report.Sliders[0] = AxisToFloat(state.Sliders[0] - short.MaxValue);
                    report.Sliders[1] = AxisToFloat(state.Sliders[1] - short.MaxValue);
                    report.Sliders[2] = AxisToFloat(state.VelocitySliders[0] - short.MaxValue);
                    report.Sliders[3] = AxisToFloat(state.VelocitySliders[1] - short.MaxValue);
                    report.Sliders[4] = AxisToFloat(state.AccelerationSliders[0] - short.MaxValue);
                    report.Sliders[5] = AxisToFloat(state.AccelerationSliders[1] - short.MaxValue);
                    report.Sliders[6] = AxisToFloat(state.ForceSliders[0] - short.MaxValue);
                    report.Sliders[7] = AxisToFloat(state.ForceSliders[1] - short.MaxValue);

                    report.SixAxis[0] = IntArrToVec(state.VelocityX, state.VelocityY, state.VelocityZ);
                    report.SixAxis[1] = IntArrToVec(state.AngularVelocityX, state.AngularVelocityY, state.AngularVelocityZ);
                    report.SixAxis[2] = IntArrToVec(state.AccelerationX, state.AccelerationY, state.AccelerationZ);
                    report.SixAxis[3] = IntArrToVec(state.AngularAccelerationX, state.AngularAccelerationY, state.AngularAccelerationZ);
                    report.Rotation = IntArrToQuat(state.ForceX, state.ForceY, state.ForceZ);
                    report.DeltaRotation = IntArrToQuat(state.TorqueX, state.TorqueY, state.TorqueZ);

                    for (int i = 0; i < state.PointOfViewControllers.Length; i++)
                    {
                        report.Povs[i].Angle = state.PointOfViewControllers[i] != -1 ? state.PointOfViewControllers[i] / 100 : -1f;
                        report.Povs[i].CalculateButtons();
                    }
                }
            }
            catch { }

            return report;
        }

        public void SendOutputReport(DSOutputReport report)
        {

        }

        private static float AxisToFloat(int axis) => (float)axis / (short.MaxValue + (axis > 0 ? 1 : 0));
        private static DSVector3 IntArrToVec(int x, int y, int z) => new(
            AxisToFloat(x - short.MaxValue), AxisToFloat(y - short.MaxValue), AxisToFloat(z - short.MaxValue));
        private static DSQuaternion IntArrToQuat(int x, int y, int z) => new(
            AxisToFloat(x - short.MaxValue), AxisToFloat(y - short.MaxValue), AxisToFloat(z - short.MaxValue), 0);
    }
}