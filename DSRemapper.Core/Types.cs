using DSRemapper.DSMath;

namespace DSRemapper.Types
{
    /// <summary>
    /// DSPov class that represent a POV (Point Of View) control (also known as D-Pad)
    /// </summary>
    public class DSPov
    {
        private float angle = -1;
        /// <summary>
        /// Current angle (in degrees) of the POV control
        /// </summary>
        public float Angle { get { return angle; } set { angle = value < 0 ? -1 : Math.Clamp(value, 0, 360); CalculateButtons(); } }
        private readonly bool[] buts = new bool[4];
        /// <summary>
        /// Gets if the pov is poiting North/Up
        /// </summary>
        public bool Up { get => buts[0]; set { buts[0] = value; Angle = -1; } }
        /// <summary>
        /// Gets if the pov is poiting East/Right
        /// </summary>
        public bool Right { get => buts[1]; set { buts[1] = value; Angle = -1; } }
        /// <summary>
        /// Gets if the pov is poiting South/Down
        /// </summary>
        public bool Down { get => buts[2]; set { buts[2] = value; Angle = -1; } }
        /// <summary>
        /// Gets if the pov is poiting West/Left
        /// </summary>
        public bool Left { get => buts[3]; set { buts[3] = value; Angle = -1; } }

        /// <summary>
        /// Auxiliar function to set pov from a 0 to 8 value (8 is 'nothing pressed')
        /// </summary>
        /// <param name="pov">A byte value within 0-8 range</param>
        public void SetDSPov(byte pov)
        {
            Angle = pov != 8 ? pov * 360f / 8f : -1;
            CalculateButtons();
        }
        /// <summary>
        /// Updates the unassigned values of the POV.
        /// If the angle is -1, the function will calculate the angle using the buttons value. Otherwise, it will calculate the buttons using the angle value.
        /// </summary>
        public void Update()
        {
            if (Angle == -1)
                CalculateAngle();
            else
                CalculateButtons();
        }
        /// <summary>
        /// Calculate the angle using the buttons value
        /// </summary>
        public void CalculateAngle()
        {
            if (Up && !Right && !Down && !Left)
                Angle = 0;
            else if (Up && Right && !Down && !Left)
                Angle = 45;
            else if (!Up && Right && !Down && !Left)
                Angle = 90;
            else if (!Up && Right && Down && !Left)
                Angle = 135;
            else if (!Up && !Right && Down && !Left)
                Angle = 180;
            else if (!Up && !Right && Down && Left)
                Angle = 225;
            else if (!Up && !Right && !Down && Left)
                Angle = 270;
            else if (Up && !Right && !Down && Left)
                Angle = 315;
            else
                Angle = -1;
        }
        /// <summary>
        /// Calculate the buttons using the angle value
        /// </summary>
        public void CalculateButtons()
        {
            int dPad = (int)(Angle / 45f);
            if (Angle >= 0)
            {
                int bitSwap = dPad / 2;
                dPad = ((1 << bitSwap) | ((dPad % 2 << (bitSwap + 1)) % 15)) & 0b1111;
            }
            else
                dPad = 0;

            buts[0] = Convert.ToBoolean(dPad & (1 << 0));
            buts[1] = Convert.ToBoolean(dPad & (1 << 1));
            buts[2] = Convert.ToBoolean(dPad & (1 << 2));
            buts[3] = Convert.ToBoolean(dPad & (1 << 3));
        }
        /// <summary>
        /// String representation of the pressed buttons of the POV/D-Pad
        /// </summary>
        /// <returns>A string representing the buttons values of the POV</returns>
        public override string ToString() => $"U:{Up},D:{Down},L:{Left},R:{Right}";
        /// <summary>
        /// String representation of the angle of the POV/D-Pad
        /// </summary>
        /// <returns>A string containing the angle in degrees</returns>
        public string ToStringAngle() => $"Ang: {Angle}";
    }
    /// <summary>
    /// DSLight class that represents the DualShock 4 light bar
    /// </summary>
    public class DSLight
    {
        private readonly float[] led = new float[3] { 0f, 0f, 0f };
        private readonly float[] OnOff = new float[2] { 0f, 0f };
        /// <summary>
        /// Player number for Xbox controller compatibility, to get or set the player number of the controller.
        /// </summary>
        public float Player { get; set; }
        /// <summary>
        /// Red value of the RGB of the light bar (range: 0-1)
        /// </summary>
        public float Red { get { return led[0]; } set { led[0] = Math.Clamp(value, 0, 1); } }
        /// <summary>
        /// Green value of the RGB of the light bar (range: 0-1)
        /// </summary>
        public float Green { get { return led[1]; } set { led[1] = Math.Clamp(value, 0, 1); } }
        /// <summary>
        /// Blue value of the RGB of the light bar (range: 0-1)
        /// </summary>
        public float Blue { get { return led[2]; } set { led[2] = Math.Clamp(value, 0, 1); } }
        /// <summary>
        /// On time percentage for the DualShock 4 light bar (range: 0-1)
        /// If this property and 'OffTime' property are both 0, light bar is always on.
        /// </summary>
        public float OnTime { get { return OnOff[0]; } set { OnOff[0] = Math.Clamp(value, 0, 1); } }
        /// <summary>
        /// Off time percentage for the DualShock 4 light bar (range: 0-1)
        /// If this property and 'OnTime' property are both 0, light bar is always on.
        /// </summary>
        public float OffTime { get { return OnOff[1]; } set { OnOff[1] = Math.Clamp(value, 0, 1); } }
        /// <summary>
        /// DSLight class contructor
        /// </summary>
        public DSLight() { }
        /// <summary>
        /// DSLight class contructor
        /// </summary>
        /// <param name="red">Red value of the light bar (range: 0-1)</param>
        /// <param name="green">Green value of the light bar (range: 0-1)</param>
        /// <param name="blue">Blue value of the light bar (range: 0-1)</param>
        /// <param name="intensity">Global multiplier of intensity/brightness for all the color channels (RGB values)</param>
        public DSLight(float red, float green, float blue, float intensity = 1) => SetRGB(red, green, blue, intensity);
        /// <summary>
        /// Sets all the color channels of the light bar, and applies an intesity/brightness value
        /// </summary>
        /// <param name="red">Red value of the light bar (range: 0-1)</param>
        /// <param name="green">Green value of the light bar (range: 0-1)</param>
        /// <param name="blue">Blue value of the light bar (range: 0-1)</param>
        /// <param name="intensity">Global multiplier of intensity for all the color channels (RGB values)</param>
        public void SetRGB(float red, float green, float blue, float intensity = 1)
        {
            Red = red * intensity;
            Green = green * intensity;
            Blue = blue * intensity;
        }
        /// <summary>
        /// Sets all the color channels of the light bar, and applies an intesity/brightness value
        /// </summary>
        /// <param name="leds">An array, of at least, three values representing the RGB channels (each array value range: 0-1)</param>
        /// <param name="intensity">Global multiplier of intensity for all the color channels (RGB values)</param>
        public void SetRGB(float[] leds, float intensity = 1)
        {
            if (leds.Length >= 3)
            {
                Red = leds[0] * intensity;
                Green = leds[1] * intensity;
                Blue = leds[2] * intensity;
            }
        }
        /// <summary>
        /// Multiplies the current led color channels by an intensity/brightness value
        /// </summary>
        /// <param name="light">The DSLight class to apply the intensity</param>
        /// <param name="intensity">The intensity value</param>
        /// <returns>A new DSLight object with the intensity/brightness applied</returns>
        public static DSLight operator *(DSLight light, float intensity) => new(light.Red, light.Green, light.Blue, intensity);
        /// <inheritdoc cref="operator *(DSLight, float)"/>
        public static DSLight operator *(float intensity, DSLight light) => light * intensity;
        /// <summary>
        /// Adds the value to the current led color channels values
        /// </summary>
        /// <param name="light">The DSLight class to apply the intensity</param>
        /// <param name="add">The value to add to all color channels</param>
        /// <returns>A new DSLight object with the value added to all color channels</returns>
        public static DSLight operator +(DSLight light, float add) => new(light.Red + add, light.Green + add, light.Blue + add);
        /// <inheritdoc cref="operator +(DSLight, float)"/>
        public static DSLight operator +(float add, DSLight light) => light + add;
        /// <summary>
        /// Subtract the value to the current led color channels values
        /// </summary>
        /// <param name="light">The DSLight class to apply the intensity</param>
        /// <param name="sub">The value to subtracted to all color channels</param>
        /// <returns>A new DSLight object with the value subtracted to all color channels</returns>
        public static DSLight operator -(DSLight light, float sub) => light + (-sub);
        /// <inheritdoc cref="operator -(DSLight, float)"/>
        public static DSLight operator -(float sub, DSLight light) => light - sub;
    }
    /// <summary>
    /// DSTouch class that represents a finger touch on a touchpad (intended for the DualShock 4 touchpad)
    /// </summary>
    public class DSTouch
    {
        /// <summary>
        /// Current id of the touch
        /// </summary>
        public int Id { get; set; } = 0;
        /// <summary>
        /// Gets if the finger is touching the touchpad
        /// </summary>
        public bool Pressed { get; set; } = false;
        /// <summary>
        /// A 2D vector representing the position of the finger in a range of 0-1 in both axis
        /// </summary>
        public DSVector2 Pos { get; set; } = new();
        /// <summary>
        /// Gets a String representation of the DSTouch class
        /// </summary>
        /// <returns>A string containing the id, if is pressed and the position of the touch</returns>
        public override string ToString() => $"Id:{Id},P:{Pressed},{Pos}";
    }
    /*public interface IDSInputReport
    {
        public virtual float Battery { get => 0; }
        public virtual bool Usb { get => false; }
        public float[] Axes { get; }
        public float[] Sliders { get; }
        public bool[] Buttons { get; }
        public DSPov[] Povs { get; }
        public DSVector3[] SixAxes { get; }
        public DSQuaternion[] Quaternions { get; }
        public DSTouch[] Touch { get; }
        public DSVector2 TouchPadSize { get; }

        public virtual void SetAxes(float[] axis) => SetAxes(axis, 0, axis.Length);
        public virtual void SetAxes(float[] axis, int offset, int length)
        {
            int runLength = Math.Min(Axes.Length - offset, length);

            for (int i = offset; i < runLength; i++)
                Axes[i] = axis[i];
        }
        public virtual void SetSliders(float[] sliders) => SetSliders(sliders, 0, sliders.Length);
        public virtual void SetSliders(float[] sliders, int offset, int length)
        {
            int runLength = Math.Min(Sliders.Length - offset, length);

            for (int i = offset; i < runLength; i++)
                Sliders[i] = sliders[i];
        }
        public virtual void SetButtons(bool[] buttons) => SetButtons(buttons, 0, buttons.Length);
        public virtual void SetButtons(bool[] buttons, int offset, int length)
        {
            int runLength = Math.Min(Buttons.Length - offset, length);

            for (int i = offset; i < runLength; i++)
                Buttons[i] = buttons[i];
        }
        public virtual void SetPovs(DSPov[] povs) => SetPovs(povs, 0, povs.Length);
        public virtual void SetPovs(DSPov[] povs, int offset, int length)
        {
            int runLength = Math.Min(Povs.Length - offset, length);

            for (int i = 0; i < runLength; i++)
                Povs[i] = povs[i];
        }
        public virtual void SetTouchPads(DSTouch[] touch) => SetTouchPads(touch, 0, touch.Length);
        public virtual void SetTouchPads(DSTouch[] touch, int offset, int length)
        {
            int runLength = Math.Min(Touch.Length - offset, length);

            for (int i = 0; i < runLength; i++)
                Touch[i] = touch[i];
        }

        #region Axes
        /// <summary>
        /// Left Stick X Axis (for DS4 and Xbox)
        /// </summary>
        public virtual float LX { get { return Axes[0]; } set { Axes[0] = value; } }
        /// <summary>
        /// Left Stick Y Axis (for DS4 and Xbox)
        /// </summary>
        public virtual float LY { get { return Axes[1]; } set { Axes[1] = value; } }
        /// <summary>
        /// Right Stick X Axis (for DS4 and Xbox)
        /// </summary>
        public virtual float RX { get { return Axes[2]; } set { Axes[2] = value; } }
        /// <summary>
        /// Right Stick Y Axis (for DS4 and Xbox)
        /// </summary>
        public virtual float RY { get { return Axes[3]; } set { Axes[3] = value; } }
        /// <summary>
        /// Left Trigger Axis (for DS4 and Xbox)
        /// </summary>
        public virtual float LTrigger { get { return Axes[4]; } set { Axes[4] = value; } }
        /// <summary>
        /// Right Trigger Axis (for DS4 and Xbox)
        /// </summary>
        public virtual float RTrigger { get { return Axes[5]; } set { Axes[5] = value; } }
        #endregion Axes

        #region POV1
        /// <summary>
        /// Controller First Pov
        /// </summary>
        public virtual DSPov Pov { get => Povs[0]; set => Povs[0] = value; }
        /// <summary>
        /// Controller First Pov Up Button
        /// </summary>
        public virtual bool Up { get { return Povs[0].Up; } set { Povs[0].Up = value; } }
        /// <summary>
        /// Controller First Pov Right Button
        /// </summary>
        public virtual bool Right { get { return Povs[0].Right; } set { Povs[0].Right = value; } }
        /// <summary>
        /// Controller First Pov Down Button
        /// </summary>
        public virtual bool Down { get { return Povs[0].Down; } set { Povs[0].Down = value; } }
        /// <summary>
        /// Controller First Pov Left Button
        /// </summary>
        public virtual bool Left { get { return Povs[0].Left; } set { Povs[0].Left = value; } }
        #endregion POV1

        #region DS4Layout
        public virtual bool Square { get { return Buttons[0]; } set { Buttons[0] = value; } }
        public virtual bool Cross { get { return Buttons[1]; } set { Buttons[1] = value; } }
        public virtual bool Circle { get { return Buttons[2]; } set { Buttons[2] = value; } }
        public virtual bool Triangle { get { return Buttons[3]; } set { Buttons[3] = value; } }
        public virtual bool L1 { get { return Buttons[4]; } set { Buttons[4] = value; } }
        public virtual bool R1 { get { return Buttons[5]; } set { Buttons[5] = value; } }
        public virtual bool L2 { get { return Buttons[6]; } set { Buttons[6] = value; } }
        public virtual bool R2 { get { return Buttons[7]; } set { Buttons[7] = value; } }
        public virtual bool Share { get { return Buttons[8]; } set { Buttons[8] = value; } }
        public virtual bool Options { get { return Buttons[9]; } set { Buttons[9] = value; } }
        public virtual bool L3 { get { return Buttons[10]; } set { Buttons[10] = value; } }
        public virtual bool R3 { get { return Buttons[11]; } set { Buttons[11] = value; } }
        public virtual bool PS { get { return Buttons[12]; } set { Buttons[12] = value; } }
        public virtual bool TouchClick { get { return Buttons[13]; } set { Buttons[13] = value; } }
        #endregion DS4Layout

        #region XboxLayout
        public virtual bool X { get { return Buttons[0]; } set { Buttons[0] = value; } }
        public virtual bool A { get { return Buttons[1]; } set { Buttons[1] = value; } }
        public virtual bool B { get { return Buttons[2]; } set { Buttons[2] = value; } }
        public virtual bool Y { get { return Buttons[3]; } set { Buttons[3] = value; } }
        public virtual bool LButton { get { return Buttons[4]; } set { Buttons[4] = value; } }
        public virtual bool RButton { get { return Buttons[5]; } set { Buttons[5] = value; } }
        public virtual bool Back { get { return Buttons[8]; } set { Buttons[8] = value; } }
        public virtual bool Start { get { return Buttons[9]; } set { Buttons[9] = value; } }
        public virtual bool LThumb { get { return Buttons[10]; } set { Buttons[10] = value; } }
        public virtual bool RThumb { get { return Buttons[11]; } set { Buttons[11] = value; } }
        public virtual bool Guide { get { return Buttons[12]; } set { Buttons[12] = value; } }
        #endregion XboxLayout

        #region SixAxes
        public virtual DSVector3 RawAccel { get { return SixAxes[0]; } set { SixAxes[0] = value; } }
        public virtual DSVector3 Gyro { get { return SixAxes[1]; } set { SixAxes[1] = value; } }
        public virtual DSVector3 Grav { get { return SixAxes[2]; } set { SixAxes[2] = value; } }
        public virtual DSVector3 Accel { get { return SixAxes[3]; } set { SixAxes[3] = value; } }
        public virtual DSQuaternion DeltaRotation { get { return Quaternions[0]; } set { Quaternions[0] = value; } }
        public virtual DSQuaternion Rotation { get { return Quaternions[1]; } set { Quaternions[1] = value; } }
        #endregion SixAxes
    }*/
    /// <summary>
    /// Standardization of input report for DSRemapper plugins framework. Is used for DSRemapper remap profiles as well.
    /// </summary>
    public class DSInputReport
    {
        /// <summary>
        /// Gets the battery level of the controller (intended for wireless controllers as DualShock 4 or similars)
        /// </summary>
        public float Battery { get; set; } = 0;
        /// <summary>
        /// Gets if the controller is currently charging (intended for the DualShock 4, which has a value indicating if it is charging/connected to an usb cable)
        /// </summary>
        public bool Usb { get; set; } = false;
        /// <summary>
        /// Same as Usb property, but with a friendly name
        /// </summary>
        public bool Charging { get => Usb; set => Usb = value; }
        /// <summary>
        /// Readonly definition for Axis array for backwards compatibility with old remap Profiles
        /// </summary>
        [Obsolete("It's a redefinition of 'Axes' property for old remap profiles, better use 'Axes' property")]
        public float[] Axis => Axes;
        /// <summary>
        /// Axes array containing all the axes of the controller
        /// </summary>
        public float[] Axes { get; } = new float[6];
        /// <summary>
        /// Sliders array containing all the sliders of the controller
        /// </summary>
        public float[] Sliders { get; } = Array.Empty<float>();
        /// <summary>
        /// Buttons array containing all the buttons of the controller
        /// </summary>
        public bool[] Buttons { get; } = new bool[14];
        /// <summary>
        /// POVs array containing all the POVs/D-Pads of the controller
        /// </summary>
        public DSPov[] Povs { get; } = new DSPov[1] { new() };
        /// <summary>
        /// SixAxis array containing all the IMU data of the controller
        /// </summary>
        public DSVector3[] SixAxis { get; } = new DSVector3[4] { new(), new(), new(), new() };
        /// <summary>
        /// Quaternions array containing IMU data quaternions of the controller
        /// </summary>
        public DSQuaternion[] Quaternions { get; } = new DSQuaternion[2] { new(), new() };
        /// <summary>
        /// Gets the delta/diference of quaternion rotation from the last report
        /// </summary>
        public DSQuaternion DeltaRotation { get=> Quaternions[0]; set=> Quaternions[0]=value; }
        /// <summary>
        /// Gets the current total rotation of the controller as a quaternion
        /// </summary>
        public DSQuaternion Rotation { get => Quaternions[1]; set => Quaternions[1] = value; }
        /// <summary>
        /// Touches array containing all the finger touchs on the controller touchpad
        /// </summary>
        public DSTouch[] Touch { get; } = new DSTouch[2] { new(), new() };
        /// <summary>
        /// Touchpad resolution as a 2D vector.
        /// </summary>
        public DSVector2 TouchPadSize { get; set; } = new();
        /// <summary>
        /// Delta time from last report. Set by input plugins with the delta time used to calculate IMU data.
        /// This field can be always 0, if the input plugin doesn't update it.
        /// </summary>
        public float deltaTime = 0;

        /// <summary>
        /// DSInputReport class contructor
        /// </summary>
        public DSInputReport() { }
        /// <summary>
        /// DSInputReport class contructor
        /// </summary>
        /// <param name="axes">Number of axes for the structure</param>
        /// <param name="sliders">Number of sliders for the structure</param>
        /// <param name="buttons">Number of buttons for the structure</param>
        /// <param name="povs">Number of povs for the structure</param>
        /// <param name="touchs">Number of touch structures for the structure</param>
        public DSInputReport(byte axes = 6, byte sliders = 0, byte buttons = 14, byte povs = 1, byte touchs = 2)
        {
            Axes = new float[axes];
            Sliders = new float[sliders];
            Buttons = new bool[buttons];

            povs = Math.Clamp(povs, (byte)1, (byte)4);
            Povs = new DSPov[povs];
            for (int i = 0; i < Povs.Length; i++)
                Povs[i] = new DSPov();

            Touch = new DSTouch[touchs];
            for (int i = 0; i < Touch.Length; i++)
                Touch[i] = new DSTouch();
        }
        /// <summary>
        /// Definition for SetAxes function for backwards compatibility with old remap Profiles
        /// </summary>
        [Obsolete("It's a redefinition of 'SetAxes' function for old remap profiles, better use 'SetAxes' function")]
        public void SetAxis(float[] axes) => SetAxes(axes);
        /// <summary>
        /// Sets all axes to the shortest length between controller axes or input array
        /// </summary>
        /// <param name="axes">Axes array to set to the structure</param>
        public void SetAxes(float[] axes)
        {
            int length = Math.Min(Axes.Length, axes.Length);

            for (int i = 0; i < length; i++)
                Axes[i] = axes[i];
        }
        /// <summary>
        /// Sets all sliders to the shortest length between controller sliders or input array
        /// </summary>
        /// <param name="sliders">Sliders array to set to the structure</param>
        public void SetSliders(float[] sliders)
        {
            int length = Math.Min(Sliders.Length, sliders.Length);

            for (int i = 0; i < length; i++)
                Sliders[i] = sliders[i];
        }
        /// <summary>
        /// Sets all buttons to the shortest length between controller buttons or input array
        /// </summary>
        /// <param name="buttons">Buttons array to set to the structure</param>
        public void SetButtons(bool[] buttons) => SetButtons(buttons, 0, buttons.Length);
        /// <summary>
        /// Sets all buttons from the offset index to the length index or the final index of the controller buttons array (if the length is over the input array length will result in an error)
        /// </summary>
        /// <param name="buttons">Buttons array to set to the structure</param>
        /// <param name="offset">Starting index</param>
        /// <param name="length">Final index</param>
        public void SetButtons(bool[] buttons, int offset, int length)
        {
            int runLength = Math.Min(Buttons.Length - offset, length);

            for (int i = offset; i < runLength; i++)
                Buttons[i] = buttons[i];
        }
        /// <summary>
        /// Sets all POVs to the shortest length between controller POVs or input array
        /// </summary>
        /// <param name="povs">POVs array to set to the structure</param>
        public void SetPovs(DSPov[] povs)
        {
            int length = Math.Min(Povs.Length, povs.Length);

            for (int i = 0; i < length; i++)
                Povs[i] = povs[i];
        }
        /// <summary>
        /// Sets all touches to the shortest length between controller touches or input array
        /// </summary>
        /// <param name="touch">Touches array to set to the structure</param>
        public void SetTouchPads(DSTouch[] touch)
        {
            int length = Math.Min(Touch.Length, touch.Length);

            for (int i = 0; i < length; i++)
                Touch[i] = touch[i];
        }

        #region Axis
        /// <summary>
        /// Left Stick X Axis (for DS4 and Xbox)
        /// </summary>
        public virtual float LX { get { return Axes[0]; } set { Axes[0] = value; } }
        /// <summary>
        /// Left Stick Y Axis (for DS4 and Xbox)
        /// </summary>
        public virtual float LY { get { return Axes[1]; } set { Axes[1] = value; } }
        /// <summary>
        /// Right Stick X Axis (for DS4 and Xbox)
        /// </summary>
        public virtual float RX { get { return Axes[2]; } set { Axes[2] = value; } }
        /// <summary>
        /// Right Stick Y Axis (for DS4 and Xbox)
        /// </summary>
        public virtual float RY { get { return Axes[3]; } set { Axes[3] = value; } }
        /// <summary>
        /// Left Trigger Axis (for DS4 and Xbox)
        /// </summary>
        public virtual float LTrigger { get { return Axes[4]; } set { Axes[4] = value; } }
        /// <summary>
        /// Right Trigger Axis (for DS4 and Xbox)
        /// </summary>
        public virtual float RTrigger { get { return Axes[5]; } set { Axes[5] = value; } }
        #endregion Axis

        #region POV1
        /// <summary>
        /// Controller main/first POV/D-Pad
        /// </summary>
        public DSPov Pov { get => Povs[0]; set => Povs[0] = value; }
        /// <summary>
        /// Controller main/first POV up button
        /// </summary>
        public bool Up { get { return Povs[0].Up; } set { Povs[0].Up = value; } }
        /// <summary>
        /// Controller main/first POV right button
        /// </summary>
        public bool Right { get { return Povs[0].Right; } set { Povs[0].Right = value; } }
        /// <summary>
        /// Controller main/first POV down button
        /// </summary>
        public bool Down { get { return Povs[0].Down; } set { Povs[0].Down = value; } }
        /// <summary>
        /// Controller main/first POV left button
        /// </summary>
        public bool Left { get { return Povs[0].Left; } set { Povs[0].Left = value; } }
        #endregion POV1

        #region DS4Layout
        /// <summary>
        /// DualShock 4 Square button (index on the buttons array: 0)
        /// </summary>
        public bool Square { get { return Buttons[0]; } set { Buttons[0] = value; } }
        /// <summary>
        /// DualShock 4 Cross button (index on the buttons array: 1)
        /// </summary>
        public bool Cross { get { return Buttons[1]; } set { Buttons[1] = value; } }
        /// <summary>
        /// DualShock 4 Circle button (index on the buttons array: 2)
        /// </summary>
        public bool Circle { get { return Buttons[2]; } set { Buttons[2] = value; } }
        /// <summary>
        /// DualShock 4 Triangle button (index on the buttons array: 3)
        /// </summary>
        public bool Triangle { get { return Buttons[3]; } set { Buttons[3] = value; } }
        /// <summary>
        /// DualShock 4 L1 button (index on the buttons array: 4)
        /// </summary>
        public bool L1 { get { return Buttons[4]; } set { Buttons[4] = value; } }
        /// <summary>
        /// DualShock 4 R1 button (index on the buttons array: 5)
        /// </summary>
        public bool R1 { get { return Buttons[5]; } set { Buttons[5] = value; } }
        /// <summary>
        /// DualShock 4 L2 button (index on the buttons array: 6)
        /// </summary>
        public bool L2 { get { return Buttons[6]; } set { Buttons[6] = value; } }
        /// <summary>
        /// DualShock 4 R2 button (index on the buttons array: 7)
        /// </summary>
        public bool R2 { get { return Buttons[7]; } set { Buttons[7] = value; } }
        /// <summary>
        /// DualShock 4 Share button (index on the buttons array: 8)
        /// </summary>
        public bool Share { get { return Buttons[8]; } set { Buttons[8] = value; } }
        /// <summary>
        /// DualShock 4 Options button (index on the buttons array: 9)
        /// </summary>
        public bool Options { get { return Buttons[9]; } set { Buttons[9] = value; } }
        /// <summary>
        /// DualShock 4 L3 button (index on the buttons array: 10)
        /// </summary>
        public bool L3 { get { return Buttons[10]; } set { Buttons[10] = value; } }
        /// <summary>
        /// DualShock 4 R3 button (index on the buttons array: 11)
        /// </summary>
        public bool R3 { get { return Buttons[11]; } set { Buttons[11] = value; } }
        /// <summary>
        /// DualShock 4 PS button (index on the buttons array: 12)
        /// Be careful, if the button is pressed for 10 seconds the DualShock 4 controller will shutdown.
        /// </summary>
        public bool PS { get { return Buttons[12]; } set { Buttons[12] = value; } }
        /// <summary>
        /// DualShock 4 Touch Pad button (index on the buttons array: 13)
        /// </summary>
        public bool TouchPad { get { return Buttons[13]; } set { Buttons[13] = value; } }
        /// <summary>
        /// Same as Touch Pad button (index on the buttons array: 13). Is declared for backwards compatibility
        /// </summary>
        [Obsolete("It's a redefinition of 'TouchPad' property for old remap profiles, better use 'TouchPad' property")]
        public bool TouchClick { get { return Buttons[13]; } set { Buttons[13] = value; } }
        #endregion DS4Layout

        #region XboxLayout
        /// <summary>
        /// Xbox X button (index on the buttons array: 0)
        /// </summary>
        public bool X { get { return Buttons[0]; } set { Buttons[0] = value; } }
        /// <summary>
        /// Xbox A button (index on the buttons array: 1)
        /// </summary>
        public bool A { get { return Buttons[1]; } set { Buttons[1] = value; } }
        /// <summary>
        /// Xbox B button (index on the buttons array: 2)
        /// </summary>
        public bool B { get { return Buttons[2]; } set { Buttons[2] = value; } }
        /// <summary>
        /// Xbox Y button (index on the buttons array: 3)
        /// </summary>
        public bool Y { get { return Buttons[3]; } set { Buttons[3] = value; } }
        /// <summary>
        /// Xbox Left Button button (index on the buttons array: 4)
        /// </summary>
        public bool LButton { get { return Buttons[4]; } set { Buttons[4] = value; } }
        /// <summary>
        /// Xbox Right Button button (index on the buttons array: 5)
        /// </summary>
        public bool RButton { get { return Buttons[5]; } set { Buttons[5] = value; } }
        /// <summary>
        /// Xbox Back button (index on the buttons array: 8)
        /// </summary>
        public bool Back { get { return Buttons[8]; } set { Buttons[8] = value; } }
        /// <summary>
        /// Xbox Start button (index on the buttons array: 9)
        /// </summary>
        public bool Start { get { return Buttons[9]; } set { Buttons[9] = value; } }
        /// <summary>
        /// Xbox Left Thumb button (index on the buttons array: 10)
        /// </summary>
        public bool LThumb { get { return Buttons[10]; } set { Buttons[10] = value; } }
        /// <summary>
        /// Xbox Right Thumb button (index on the buttons array: 11)
        /// </summary>
        public bool RThumb { get { return Buttons[11]; } set { Buttons[11] = value; } }
        /// <summary>
        /// Xbox Guide button (index on the buttons array: 12)
        /// </summary>
        public bool Guide { get { return Buttons[12]; } set { Buttons[12] = value; } }
        #endregion XboxLayout

        #region SixAxis
        /// <summary>
        /// Raw Acceleration data (with gravity) from IMU data (index on SixAxis array: 0)
        /// </summary>
        public DSVector3 RawAccel { get { return SixAxis[0]; } set { SixAxis[0] = value; } }
        /// <summary>
        /// Raw Gyro data from IMU data (index on SixAxis array: 1)
        /// </summary>
        public DSVector3 Gyro { get { return SixAxis[1]; } set { SixAxis[1] = value; } }
        /// <summary>
        /// Gravity vector from IMU data (index on SixAxis array: 2)
        /// Points to the "global/planet" gravity relative to the controller.
        /// </summary>
        public DSVector3 Grav { get { return SixAxis[2]; } set { SixAxis[2] = value; } }
        /// <summary>
        /// Processed Acceleration data (without gravity and fixed) from IMU data (index on SixAxis array: 3)
        /// Rotation of the controller is irrelevant to this value. If the controller is turned 90 degrees, and is moved left to right, in this vector it will still be on the X axis.
        /// </summary>
        public DSVector3 Accel { get { return SixAxis[3]; } set { SixAxis[3] = value; } }
        #endregion SixAxis
    }
    /// <summary>
    /// Standardization of output report for DSRemapper plugins framework. Is used for DSRemapper remap profiles as well.
    /// </summary>
    public class DSOutputReport
    {
        /// <summary>
        /// Constant for default DualShock 4 ligth bar intensity
        /// </summary>
        private const float defaulLedIntensity = 0.125f;
        /// <summary>
        /// Motor rumble array for controller vibration. It's functionality may vary depending on how the plugins use it.
        /// </summary>
        public float[] Rumble { get; set; } = new float[6];
        /// <summary>
        /// DSLight class representing DualShock 4 light bar
        /// </summary>
        public DSLight Led { get; set; } = new();
        /// <summary>
        /// Extension values for leds. It's functionality may vary depending on how the plugins use it.
        /// </summary>
        public float[] ExtLeds { get; set; } = new float[6];
        /// <summary>
        /// DSOutputReport class constructor
        /// </summary>
        public DSOutputReport() {
            Led.Player = 1;
            Red = 0.4f * defaulLedIntensity;
            Green = 0.8f * defaulLedIntensity;
            Blue = 1f * defaulLedIntensity;
        }
        /// <summary>
        /// Right rumble motor of the controller (index on rumble array: 0)
        /// </summary>
        public float Right { get { return Rumble[0]; } set { Rumble[0] = value; } }
        /// <summary>
        /// Left rumble motor of the controller (index on rumble array: 1)
        /// </summary>
        public float Left { get { return Rumble[1]; } set { Rumble[1] = value; } }
        #region DS4Layout
        /// <summary>
        /// Weak/right rumble motor of a DualShock 4 controller (index on rumble array: 0)
        /// </summary>
        public float Weak { get { return Rumble[0]; } set { Rumble[0] = value; } }
        /// <summary>
        /// Strong/left rumble motor of a DualShock 4 controller (index on rumble array: 1)
        /// </summary>
        public float Strong { get { return Rumble[1]; } set { Rumble[1] = value; } }
        /// <summary>
        /// Red led value for the DualShock 4 light bar (range: 0-1)
        /// </summary>
        public float Red { get { return Led.Red; } set { Led.Red = value; } }
        /// <summary>
        /// Green led value for the DualShock 4 light bar (range: 0-1)
        /// </summary>
        public float Green { get { return Led.Green; } set { Led.Green = value; } }
        /// <summary>
        /// Blue led value for the DualShock 4 light bar (range: 0-1)
        /// </summary>
        public float Blue { get { return Led.Blue; } set { Led.Blue = value; } }
        /// <summary>
        /// On time percentage for the DualShock 4 light bar (range: 0-1)
        /// If this property and 'OffTime' property are both 0, light bar is always on.
        /// </summary>
        public float OnTime { get { return Led.OnTime; } set { Led.OnTime = value; } }
        /// <summary>
        /// Off time percentage for the DualShock 4 light bar (range: 0-1)
        /// If this property and 'OnTime' property are both 0, light bar is always on.
        /// </summary>
        public float OffTime { get { return Led.OffTime; } set { Led.OffTime = value; } }
        #endregion DS4Layout
    }
}
