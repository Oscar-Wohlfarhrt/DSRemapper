using DSRemapper.Core;
using DSRemapper.Types;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace DSRemapper.ViGEm
{
    [EmulatedController("ViGEm/DS4")]
    internal class Xbox : IDSOutputController
    {
        private readonly IXbox360Controller emuController;
        public bool IsConnected { get; private set; }

        public DSInputReport state => new DSInputReport(6,0,14,1,0);
        private DSOutputReport feedback = new();
        public Xbox()
        {
            ViGEmClient cli = new ViGEmClient();
            emuController=cli.CreateXbox360Controller();
            emuController.FeedbackReceived += EmuController_FeedbackReceived;
        }
        public void Connect()
        {
            if (!IsConnected)
            {
                emuController.AutoSubmitReport = false;
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
            throw new NotImplementedException();
        }
        private void EmuController_FeedbackReceived(object sender, Xbox360FeedbackReceivedEventArgs e)
        {
            feedback.Weak = e.SmallMotor / 255f;
            feedback.Strong = e.LargeMotor / 255f;
            switch (feedback.Led.Player = e.LedNumber)
            {
                case 0:
                    feedback.Led.SetRGB(0, 0, 1);
                    break;
                case 1:
                    feedback.Led.SetRGB(1, 0, 0);
                    break;
                case 2:
                    feedback.Led.SetRGB(0, 1, 0);
                    break;
                case 3:
                    feedback.Led.SetRGB(1, 0, 1);
                    break;
            }
        }
        public DSOutputReport GetFeedbackReport()
        {
            return feedback;
        }

        public void Update()
        {
            if (IsConnected)
            {
                emuController.SetAxisValue(Xbox360Axis.LeftThumbX, state.LX.ToShortAxis());
                emuController.SetAxisValue(Xbox360Axis.LeftThumbY, (-state.LY).ToShortAxis());
                emuController.SetAxisValue(Xbox360Axis.RightThumbX, state.RX.ToShortAxis());
                emuController.SetAxisValue(Xbox360Axis.RightThumbY, (-state.RY).ToShortAxis());

                emuController.SetSliderValue(Xbox360Slider.LeftTrigger, state.LTrigger.ToByteTrigger());
                emuController.SetSliderValue(Xbox360Slider.RightTrigger, state.RTrigger.ToByteTrigger());

                emuController.SetButtonState(Xbox360Button.A, state.A);
                emuController.SetButtonState(Xbox360Button.B, state.B);
                emuController.SetButtonState(Xbox360Button.X, state.X);
                emuController.SetButtonState(Xbox360Button.Y, state.Y);

                emuController.SetButtonState(Xbox360Button.Up, state.Up);
                emuController.SetButtonState(Xbox360Button.Left, state.Left);
                emuController.SetButtonState(Xbox360Button.Down, state.Down);
                emuController.SetButtonState(Xbox360Button.Right, state.Right);

                emuController.SetButtonState(Xbox360Button.LeftShoulder, state.LButton);
                emuController.SetButtonState(Xbox360Button.RightShoulder, state.RButton);
                emuController.SetButtonState(Xbox360Button.LeftThumb, state.LThumb);
                emuController.SetButtonState(Xbox360Button.RightThumb, state.RThumb);

                emuController.SetButtonState(Xbox360Button.Back, state.Back);
                emuController.SetButtonState(Xbox360Button.Start, state.Start);
                emuController.SetButtonState(Xbox360Button.Guide, state.Guide);
                emuController.SubmitReport();
            }
        }
    }
}
