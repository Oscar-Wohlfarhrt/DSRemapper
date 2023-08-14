using VJoyWrapper;
using DSRemapper.Core;
using DSRemapper.Types;
using System.Collections;

namespace DSRemapper.VJoyCtrl
{
    /// <summary>
    /// VJoy emulated controller class
    /// </summary>
    [EmulatedController("VJoy")]
    public class VJoyCtrl : IDSOutputController
    {
        private readonly uint range;
        private readonly VJoy joy = new();
        private VJoy.JoystickState vState = new();
        private uint? id;

        private readonly DSOutputReport report = new();

        /// <inheritdoc/>
        public bool IsConnected { get; private set; }
        /// <inheritdoc/>
        public DSInputReport State { get; private set; }

        /// <summary>
        /// VJoy controller class constructor
        /// </summary>
        public VJoyCtrl()
        {
            State = new(6, 2, 128, 4, 0);
            range = 32768;
        }
        /// <summary>
        /// Connects to VJoy controller with the id 1
        /// </summary>
        public void Connect()
        {
            Connect(1);
        }
        /// <summary>
        /// Connects to VJoy controller with the specified id
        /// </summary>
        /// <param name="id">The id of the VJoy controller</param>
        public void Connect(uint id)
        {
            if(!this.id.HasValue)
                this.id = id;

            if (joy.AcquireVJD(id))
                IsConnected = true;
        }
        /// <summary>
        /// Connects to VJoy controller with the specified id.
        /// Defined for Lua Interpreter plugin support
        /// </summary>
        /// <param name="id">The id of the VJoy controller</param>
        public void Connect(double id) => Connect((uint)id);
        /// <inheritdoc/>
        public void Disconnect()
        {
            if (id.HasValue)
            {
                joy.RelinquishVJD(id.Value);
                IsConnected = false;
            }
        }
        /// <inheritdoc/>
        public void Dispose()
        {
            Disconnect();
            GC.SuppressFinalize(this);
        }
        /// <inheritdoc/>
        public DSOutputReport GetFeedbackReport()
        {
            return report;
        }
        /// <inheritdoc/>
        public void Update()
        {
            if (IsConnected && id.HasValue)
            {
                vState.AxisX = FloatToVJoyAxis(State.Axes[0]);
                vState.AxisY = FloatToVJoyAxis(State.Axes[1]);
                vState.AxisZRot = FloatToVJoyAxis(State.Axes[2]);
                vState.AxisZ = FloatToVJoyAxis(State.Axes[3]);
                vState.AxisXRot = FloatToVJoyAxis(State.Axes[4]);
                vState.AxisYRot = FloatToVJoyAxis(State.Axes[5]);

                vState.Slider = State.Sliders[0].ToShortAxis();
                vState.Dial = State.Sliders[1].ToShortAxis();
                vState.bHats = (uint)(State.Povs[0].Angle != -1 ? State.Povs[0].Angle * 100 : -1);

                BitArray bArr = new(State.Buttons);
                uint[] buttons = new uint[4];
                bArr.CopyTo(buttons, 0);
                vState.Buttons = buttons[0];
                vState.ButtonsEx1 = buttons[1];
                vState.ButtonsEx2 = buttons[2];
                vState.ButtonsEx3 = buttons[3];
                joy.UpdateVJD(id.Value, ref vState);
            }
        }

        internal int FloatToVJoyAxis(float axis)
        {
            return (int)((axis + 1) * (axis > 0 ? range - 1 : range) / 2);
        }
    }

    internal static class FloatExtensions
    {
        public static short ToShortAxis(this float axis) => (short)(axis * (axis < 0 ? -short.MinValue : short.MaxValue));
        public static sbyte ToSByteAxis(this float axis) => (sbyte)(axis * (axis < 0 ? -sbyte.MinValue : sbyte.MaxValue));
        public static byte ToByteTrigger(this float axis) => (byte)(axis * byte.MaxValue);
    }
}