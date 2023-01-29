using VJoyWrapper;
using DSRemapper.DSInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSRemapper.Remapper;
using System.Collections;
using static System.Windows.Forms.AxHost;

namespace DSRemapper.DSOutput
{
    internal class VJEmulated : IDSOutputController, IDisposable
    {
        public bool IsConnected { get; private set; }

        public readonly DSOutputReport report = new();
        public DSInputReport state = new(axis:6,sliders: 2,buttons:32,povs:4,touchs:0);

        private readonly VJoy joy = new();
        private VJoy.JoystickState vState = new();
        private readonly uint id;

        const int negAxisConvertion = -short.MinValue;
        const int posAxisConvertion = short.MaxValue;

        public VJEmulated(uint id) {
            this.id = id;
        }

        public void Connect()
        {
            long min=0, max=0;
            joy.GetVJDAxisMin(1,HID_USAGES.HID_USAGE_X,ref min);
            joy.GetVJDAxisMax(1, HID_USAGES.HID_USAGE_X,ref max);
            Console.WriteLine($"min: {min} | max: {max}");
            if(joy.AcquireVJD(id))
                IsConnected = true;
        }

        public void Disconnect()
        {
            joy.RelinquishVJD(id);
            IsConnected = false;
        }

        public void Dispose()
        {

        }

        public DSOutputReport GetFeedbackReport()
        {
            return Utils.CreateOutputReport();
        }
        public void Update()
        {
            vState.AxisX = FloatToVJoyAxis(state.Axis[0]);
            vState.AxisY = FloatToVJoyAxis(state.Axis[1]);
            vState.AxisZRot = FloatToVJoyAxis(state.Axis[2]);
            vState.AxisZ = FloatToVJoyAxis(state.Axis[3]);
            vState.AxisXRot = FloatToVJoyAxis(state.Axis[4]);
            vState.AxisYRot = FloatToVJoyAxis(state.Axis[5]);

            vState.Slider = state.Sliders[0].ToShortAxis();
            vState.Dial = state.Sliders[1].ToShortAxis();
            vState.bHats = (uint)(state.Povs[0].Angle != -1 ? state.Povs[0].Angle * 100 : -1);

            uint[] buttons = new uint[1];
            state.Buttons.CopyTo(buttons, 0);
            vState.Buttons = buttons[0];

            joy.UpdateVJD(id, ref vState);
        }
        public void SetInputReport(DSInputReport report)
        {
            vState.AxisX = FloatToVJoyAxis(report.Axis[0]);
            vState.AxisY = FloatToVJoyAxis(report.Axis[1]);
            vState.AxisZ = FloatToVJoyAxis(report.Axis[3]);
            vState.AxisZRot = FloatToVJoyAxis(report.Axis[2]);
            vState.AxisXRot = FloatToVJoyAxis(report.Axis[4]);
            vState.AxisYRot = FloatToVJoyAxis(report.Axis[5]);

            //state.Slider = report.Sliders[0].ToShortAxis();
            //state.Dial = report.Sliders[1].ToShortAxis();
            vState.bHats = (uint)(report.Povs[0].Angle!=-1? report.Povs[0].Angle*100:-1);

            BitArray bArr = new BitArray(report.Buttons);
            uint[] buttons = new uint[1];
            bArr.CopyTo(buttons , 0);
            vState.Buttons = buttons[0];

            joy.UpdateVJD(id,ref vState);
        }


        public int FloatToVJoyAxis(float axis)
        {
            return (int)((axis + 1) * (axis > 0 ? posAxisConvertion : negAxisConvertion) / 2);
        }
    }
}
