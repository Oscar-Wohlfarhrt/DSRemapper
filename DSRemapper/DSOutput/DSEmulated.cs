using DSRemapper.DSInput;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.DualShock4;

namespace DSRemapper.DSOutput
{
    internal class DSEmulated : IDSOutputController,IDisposable
    {
        private readonly IDualShock4Controller emuController;

        private bool isConnected = false;
        public bool IsConnected { get { return isConnected; } }
        private DSOutputReport feedback = new();

        public DSEmulated(IDualShock4Controller emuController)
        {
            this.emuController = emuController;
#pragma warning disable CS0618 // El tipo o el miembro están obsoletos
            emuController.FeedbackReceived += EmuController_FeedbackReceived;
#pragma warning restore CS0618 // El tipo o el miembro están obsoletos
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

        public void Connect()
        {
            if (!isConnected)
            {
                emuController.AutoSubmitReport = false;
                emuController.Connect();
                isConnected = true;
            }
        }
        public void Disconnect()
        {
            if (isConnected)
            {
                emuController.Disconnect();
                isConnected = false;
            }
        }

        public DSOutputReport GetFeedbackReport()
        {
            /* Don't work, but it will be necesary in the future
            byte[] rawReport = emuController.AwaitRawOutputReport(5,out bool timedOut).ToArray();

            if (!timedOut)
            {
                Console.WriteLine($"feedBackId: {rawReport[0]}");
                feedback.Weak = rawReport[7] / 255f;
                feedback.Strong = rawReport[8] / 255f;
                feedback.Red = rawReport[9] / 255f;
                feedback.Green = rawReport[10] / 255f;
                feedback.Blue = rawReport[11] / 255f;
                feedback.OnTime = rawReport[12] / 255f;
                feedback.OffTime = rawReport[13] / 255f;
            }*/

            return feedback;
        }
        public void SetInputReport(DSInputReport report)
        {         
            if (isConnected)
            {
                emuController.SetAxisValue(DualShock4Axis.LeftThumbX, (byte)(report.LX.ToSByteAxis() + 128));
                emuController.SetAxisValue(DualShock4Axis.LeftThumbY, (byte)((-report.LY).ToSByteAxis() + 128));
                emuController.SetAxisValue(DualShock4Axis.RightThumbX, (byte)(report.RX.ToSByteAxis() + 128));
                emuController.SetAxisValue(DualShock4Axis.RightThumbY, (byte)((-report.RY).ToSByteAxis() + 128));

                emuController.SetSliderValue(DualShock4Slider.LeftTrigger, report.LTrigger.ToByteTrigger());
                emuController.SetSliderValue(DualShock4Slider.RightTrigger, report.RTrigger.ToByteTrigger());

                if (report.Up && !report.Left && !report.Down && !report.Right)
                    emuController.SetDPadDirection(DualShock4DPadDirection.North);
                else if (report.Up && !report.Left && !report.Down && report.Right)
                    emuController.SetDPadDirection(DualShock4DPadDirection.Northeast);
                else if (!report.Up && !report.Left && !report.Down && report.Right)
                    emuController.SetDPadDirection(DualShock4DPadDirection.East);
                else if (!report.Up && !report.Left && report.Down && report.Right)
                    emuController.SetDPadDirection(DualShock4DPadDirection.Southeast);
                else if (!report.Up && !report.Left && report.Down && !report.Right)
                    emuController.SetDPadDirection(DualShock4DPadDirection.South);
                else if (!report.Up && report.Left && report.Down && !report.Right)
                    emuController.SetDPadDirection(DualShock4DPadDirection.Southwest);
                else if (!report.Up && report.Left && !report.Down && !report.Right)
                    emuController.SetDPadDirection(DualShock4DPadDirection.West);
                else if (report.Up && report.Left && !report.Down && !report.Right)
                    emuController.SetDPadDirection(DualShock4DPadDirection.Northwest);
                else
                    emuController.SetDPadDirection(DualShock4DPadDirection.None);

                emuController.SetButtonState(DualShock4Button.Cross, report.Cross);
                emuController.SetButtonState(DualShock4Button.Circle, report.Circle);
                emuController.SetButtonState(DualShock4Button.Square, report.Square);
                emuController.SetButtonState(DualShock4Button.Triangle, report.Triangle);

                emuController.SetButtonState(DualShock4Button.Options, report.Options);
                emuController.SetButtonState(DualShock4Button.Share, report.Share);

                emuController.SetButtonState(DualShock4Button.ShoulderLeft, report.L1);
                emuController.SetButtonState(DualShock4Button.ShoulderRight, report.R1);
                emuController.SetButtonState(DualShock4Button.TriggerLeft, report.L2);
                emuController.SetButtonState(DualShock4Button.TriggerRight, report.R2);
                emuController.SetButtonState(DualShock4Button.ThumbLeft, report.L3);
                emuController.SetButtonState(DualShock4Button.ThumbRight, report.R3);

                emuController.SetButtonState(DualShock4SpecialButton.Ps, report.PS);
                emuController.SetButtonState(DualShock4SpecialButton.Touchpad, report.TouchClick);

                emuController.SubmitReport();
            }
        }
    }
}
