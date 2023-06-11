using DSRemapper.DSInput;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace DSRemapper.DSOutput
{
    internal class XboxEmulated : IDSOutputController,IDisposable
    {
        private readonly IXbox360Controller emuController;

        private bool isConnected = false;
        public bool IsConnected { get { return isConnected; } }

        public DSInputReport state { get; private set; } = new(6, 0, 13, 1, 0);
        private DSOutputReport feedback = new();

        public XboxEmulated(IXbox360Controller emuController)
        {
            this.emuController = emuController;
            this.emuController.FeedbackReceived += EmuController_FeedbackReceived;
        }
        public void Dispose()
        {
            Disconnect();
        }

        private void EmuController_FeedbackReceived(object sender, Xbox360FeedbackReceivedEventArgs e)
        {
            feedback.Weak = e.SmallMotor/255f;
            feedback.Strong = e.LargeMotor/255f;
            switch (feedback.Led.Player = e.LedNumber)
            {
                case 0:
                    feedback.Led.SetRGB(0,0,1);
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

        public void Connect()
        {
            if(!isConnected)
            {
                emuController.AutoSubmitReport=false;
                emuController.Connect();
                isConnected= true;
            }
        }
        public void Disconnect()
        {
            if(isConnected)
            {
                emuController.Disconnect();
                isConnected= false;
            }
        }

        public DSOutputReport GetFeedbackReport()
        {
            return feedback;
        }
        public void Update()
        {
            if (isConnected)
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
        public void SetInputReport(DSInputReport report)
        {
            if (isConnected)
            {
                emuController.SetAxisValue(Xbox360Axis.LeftThumbX, report.LX.ToShortAxis());
                emuController.SetAxisValue(Xbox360Axis.LeftThumbY, (-report.LY).ToShortAxis());
                emuController.SetAxisValue(Xbox360Axis.RightThumbX, report.RX.ToShortAxis());
                emuController.SetAxisValue(Xbox360Axis.RightThumbY, (-report.RY).ToShortAxis());

                emuController.SetSliderValue(Xbox360Slider.LeftTrigger, report.LTrigger.ToByteTrigger());
                emuController.SetSliderValue(Xbox360Slider.RightTrigger, report.RTrigger.ToByteTrigger());

                emuController.SetButtonState(Xbox360Button.A, report.A);
                emuController.SetButtonState(Xbox360Button.B, report.B);
                emuController.SetButtonState(Xbox360Button.X, report.X);
                emuController.SetButtonState(Xbox360Button.Y, report.Y);

                emuController.SetButtonState(Xbox360Button.Up, report.Up);
                emuController.SetButtonState(Xbox360Button.Left,report.Left);
                emuController.SetButtonState(Xbox360Button.Down, report.Down);
                emuController.SetButtonState(Xbox360Button.Right, report.Right);

                emuController.SetButtonState(Xbox360Button.LeftShoulder, report.LButton);
                emuController.SetButtonState(Xbox360Button.RightShoulder, report.RButton);
                emuController.SetButtonState(Xbox360Button.LeftThumb, report.LThumb);
                emuController.SetButtonState(Xbox360Button.RightThumb, report.RThumb);
                
                emuController.SetButtonState(Xbox360Button.Back, report.Back);
                emuController.SetButtonState(Xbox360Button.Start, report.Start);
                emuController.SetButtonState(Xbox360Button.Guide, report.Guide);
                emuController.SubmitReport();
            }
        }
    }
}
