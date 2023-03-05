using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.DirectInput;
using Windows.Gaming.Input.ForceFeedback;
using Windows.Gaming.Input;
using System.Drawing;
using System.Runtime.Intrinsics.Arm;
using Windows.Devices.HumanInterfaceDevice;
using Windows.UI.Notifications;
using static System.Windows.Forms.AxHost;

namespace DSRemapper.DSInput
{
    [Serializable]
    public class DIDeviceInfo
    {
        public DeviceInstance DeviceInstance { get; private set; }
        public Guid ProductGuid { get { return DeviceInstance.ProductGuid; } }
        public Guid InstanceGuid { get { return DeviceInstance.InstanceGuid; } }

        private byte[] ProductBytes { get { return ProductGuid.ToByteArray(); } }

        public string ProductName { get { return DeviceInstance.ProductName; } }
        public ushort ProductId { get { return BitConverter.ToUInt16(ProductBytes, 2); } }
        public ushort VendorId { get { return BitConverter.ToUInt16(ProductBytes, 0); } }

        public DIDeviceInfo(DeviceInstance deviceInstance)
        {
            DeviceInstance = deviceInstance;
        }
    }
    internal class DIController: IDSInputController
    {

        static readonly DirectInput di = new();
        public static IEnumerable<DIDeviceInfo> GetDevices(DeviceClass deviceClass, DeviceEnumerationFlags flags)
        {
            return di.GetDevices(deviceClass, flags)
                .Select((x) => { return new DIDeviceInfo(x); });
        }

        private bool isConnected = false;
        private readonly DIDeviceInfo deviceInfo;
        private readonly Joystick joy;
        private DSInputReport report = new();

        public bool IsConnected => isConnected;
        public DIDeviceInfo Information => deviceInfo;
        public DeviceProperties Properties => joy.Properties;

        public string Id => deviceInfo.InstanceGuid.ToString();
        public string ControllerName => deviceInfo.ProductName;
        public ControllerType Type => ControllerType.DI;

        public DIController(DIDeviceInfo deviceInfo)
        {
            this.deviceInfo = deviceInfo;
            joy = new Joystick(di, deviceInfo.InstanceGuid);

            report = new(sliders: 8,buttons: 32, povs: 4,touchs:0);
        }
        public void Dispose()
        {
            Disconnect();
        }
        public void Connect()
        {
            joy.Acquire();
            isConnected = true;
        }
        public void Disconnect()
        {
            isConnected = false;
            joy.Unacquire();
        }
        public void ForceDisconnect()
        {

        }
        public DSInputReport GetInputReport()
        {
            joy.Poll();
            JoystickState state = joy.GetCurrentState();

            for (int i = 0; i < report.Buttons.Length; i++)
            {
                report.Buttons[i] = state.Buttons[i];
            }

            report.Axis[0] = AxisToFloat(state.X - short.MaxValue);
            report.Axis[1] = AxisToFloat(state.Y - short.MaxValue);
            report.Axis[2] = AxisToFloat(state.Z - short.MaxValue);
            report.Axis[3] = AxisToFloat(state.RotationZ - short.MaxValue);
            report.Axis[4] = AxisToFloat(state.RotationX - short.MaxValue);
            report.Axis[5] = AxisToFloat(state.RotationY - short.MaxValue);

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

            return report;
        }
        public void SendOutputReport(DSOutputReport report)
        {

        }
        private static float AxisToFloat(int axis) => (float)axis / (short.MaxValue + (axis > 0 ? 1 : 0));
        private static DSVector3 IntArrToVec(int x, int y, int z) => new(
            AxisToFloat(x - short.MaxValue), AxisToFloat(y - short.MaxValue), AxisToFloat(z - short.MaxValue));
        private static DSQuaternion IntArrToQuat(int x, int y, int z) => new(
            AxisToFloat(x - short.MaxValue), AxisToFloat(y - short.MaxValue), AxisToFloat(z - short.MaxValue),0);
    }    
}
