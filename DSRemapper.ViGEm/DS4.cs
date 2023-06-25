using DSRemapper.Core;
using DSRemapper.Types;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.DualShock4;

namespace DSRemapper.ViGEm
{
    [EmulatedController("ViGEm/DS4")]
    public class DS4 : IDSOutputController
    {
        private readonly IDualShock4Controller emuController;
        public bool IsConnected { get; private set; }
        public DSInputReport state { get; private set; } = new(6, 0, 14, 1, 2);
        private DSOutputReport feedback = new();
        public DS4()
        {
            ViGEmClient cli = new ViGEmClient();
            emuController = cli.CreateDualShock4Controller();
#pragma warning disable CS0618 // El tipo o el miembro están obsoletos
            emuController.FeedbackReceived += EmuController_FeedbackReceived; //there is no other way
#pragma warning restore CS0618 // El tipo o el miembro están obsoletos
            emuController.AutoSubmitReport = false;
        }
        public void Connect()
        {
            if (!IsConnected)
            {
                emuController.Connect();
                IsConnected = true;
            }
        }
        public void Disconnect()
        {
            if (IsConnected)
            {
                emuController.Disconnect();
                IsConnected = false;
            }
        }
        public void Dispose()
        {
            Disconnect();
        }
        private void EmuController_FeedbackReceived(object sender, DualShock4FeedbackReceivedEventArgs e)
        {
            feedback.Weak = e.SmallMotor / 255f;
            feedback.Strong = e.LargeMotor / 255f;
            feedback.Led = new(e.LightbarColor.Red, e.LightbarColor.Green, e.LightbarColor.Blue);
        }
        public DSOutputReport GetFeedbackReport()
        {
            return feedback;
        }
        public void Update()
        {
            if (IsConnected)
            {
                emuController.SetAxisValue(DualShock4Axis.LeftThumbX, (byte)(state.LX.ToSByteAxis() + 128));
                emuController.SetAxisValue(DualShock4Axis.LeftThumbY, (byte)(state.LY.ToSByteAxis() + 128));
                emuController.SetAxisValue(DualShock4Axis.RightThumbX, (byte)(state.RX.ToSByteAxis() + 128));
                emuController.SetAxisValue(DualShock4Axis.RightThumbY, (byte)(state.RY.ToSByteAxis() + 128));

                emuController.SetSliderValue(DualShock4Slider.LeftTrigger, state.LTrigger.ToByteTrigger());
                emuController.SetSliderValue(DualShock4Slider.RightTrigger, state.RTrigger.ToByteTrigger());

                if (state.Up && !state.Left && !state.Down && !state.Right)
                    emuController.SetDPadDirection(DualShock4DPadDirection.North);
                else if (state.Up && !state.Left && !state.Down && state.Right)
                    emuController.SetDPadDirection(DualShock4DPadDirection.Northeast);
                else if (!state.Up && !state.Left && !state.Down && state.Right)
                    emuController.SetDPadDirection(DualShock4DPadDirection.East);
                else if (!state.Up && !state.Left && state.Down && state.Right)
                    emuController.SetDPadDirection(DualShock4DPadDirection.Southeast);
                else if (!state.Up && !state.Left && state.Down && !state.Right)
                    emuController.SetDPadDirection(DualShock4DPadDirection.South);
                else if (!state.Up && state.Left && state.Down && !state.Right)
                    emuController.SetDPadDirection(DualShock4DPadDirection.Southwest);
                else if (!state.Up && state.Left && !state.Down && !state.Right)
                    emuController.SetDPadDirection(DualShock4DPadDirection.West);
                else if (state.Up && state.Left && !state.Down && !state.Right)
                    emuController.SetDPadDirection(DualShock4DPadDirection.Northwest);
                else
                    emuController.SetDPadDirection(DualShock4DPadDirection.None);

                emuController.SetButtonState(DualShock4Button.Cross, state.Cross);
                emuController.SetButtonState(DualShock4Button.Circle, state.Circle);
                emuController.SetButtonState(DualShock4Button.Square, state.Square);
                emuController.SetButtonState(DualShock4Button.Triangle, state.Triangle);

                emuController.SetButtonState(DualShock4Button.Options, state.Options);
                emuController.SetButtonState(DualShock4Button.Share, state.Share);

                emuController.SetButtonState(DualShock4Button.ShoulderLeft, state.L1);
                emuController.SetButtonState(DualShock4Button.ShoulderRight, state.R1);
                emuController.SetButtonState(DualShock4Button.TriggerLeft, state.L2);
                emuController.SetButtonState(DualShock4Button.TriggerRight, state.R2);
                emuController.SetButtonState(DualShock4Button.ThumbLeft, state.L3);
                emuController.SetButtonState(DualShock4Button.ThumbRight, state.R3);

                emuController.SetButtonState(DualShock4SpecialButton.Ps, state.PS);
                emuController.SetButtonState(DualShock4SpecialButton.Touchpad, state.TouchClick);

                emuController.SubmitReport();
            }
        }
    }
}