using System.Numerics;

namespace DSRemapper
{
    public class DSPov
    {
        private float angle = 0;
        public float Angle { get { return angle; } set { angle = value < 0 ? -1 : Math.Clamp(value, 0, 360); } }
        private readonly bool[] buts = new bool[4];
        public bool Up { get => buts[0]; set { buts[0] = value; Angle = -1; } }
        public bool Right { get => buts[1]; set { buts[1] = value; Angle = -1; } }
        public bool Down { get => buts[2]; set { buts[2] = value; Angle = -1; } }
        public bool Left { get => buts[3]; set { buts[3] = value; Angle = -1; } }

        internal void SetDSPov(byte pov)
        {
            Angle = pov != 8 ? pov * 360f / 8f : -1;
            CalculateButtons();
        }

        public void Update()
        {
            if (Angle == -1)
                CalculateAngle();
            else
                CalculateButtons();
        }

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

        public override string ToString() => $"U:{Up},D:{Down},L:{Left},R:{Right}";
        public string ToStringAngle() => $"Ang: {Angle}";
    }
    public class DSLight
    {
        private readonly float[] led = new float[3] { 0f, 0f, 0f };
        private readonly float[] OnOff = new float[2] { 0f, 0f };
        public float Player { get; set; }
        public float Red { get { return led[0]; } set { led[0] = Math.Clamp(value, 0, 1); } }
        public float Green { get { return led[1]; } set { led[1] = Math.Clamp(value, 0, 1); } }
        public float Blue { get { return led[2]; } set { led[2] = Math.Clamp(value, 0, 1); } }
        public float OnTime { get { return OnOff[0]; } set { OnOff[0] = Math.Clamp(value, 0, 1); } }
        public float OffTime { get { return OnOff[1]; } set { OnOff[1] = Math.Clamp(value, 0, 1); } }

        public DSLight() { }
        public DSLight(float red, float green, float blue, float intensity = 1) => SetRGB(red, green, blue, intensity);

        public void SetRGB(float red, float green, float blue, float intensity = 1)
        {
            Red = red * intensity;
            Green = green * intensity;
            Blue = blue * intensity;
        }

        public void SetRGB(float[] leds, float intensity = 1)
        {
            if (leds.Length >= 3)
            {
                Red = leds[0] * intensity;
                Green = leds[1] * intensity;
                Blue = leds[2] * intensity;
            }
        }

        public static DSLight operator *(DSLight light, float intensity) => new(light.Red, light.Green, light.Blue, intensity);
        public static DSLight operator *(float intensity, DSLight light) => light * intensity;
        public static DSLight operator +(DSLight light, float add) => new(light.Red + add, light.Green + add, light.Blue + add);
        public static DSLight operator +(float add, DSLight light) => light + add;
        public static DSLight operator -(DSLight light, float sub) => light + (-sub);
        public static DSLight operator -(float sub, DSLight light) => light - sub;
    }
    public class DSVector2
    {
        public float X { get; set; } = 0;
        public float Y { get; set; } = 0;
        public float Length { get { return MathF.Sqrt(X * X + Y * Y); } }

        public DSVector2() { }
        public DSVector2(float value) : this(value, value) { }
        public DSVector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static implicit operator Vector2(DSVector2 vector) => new(vector.X, vector.Y);
        public static implicit operator DSVector2(Vector2 vector) => new(vector.X, vector.Y);

        public static DSVector2 operator +(DSVector2 left, DSVector2 right) => new(left.X + right.X, left.Y + right.Y);
        public static DSVector2 operator -(DSVector2 left, DSVector2 right) => new(left.X - right.X, left.Y - right.Y);
        public static DSVector2 operator *(DSVector2 left, DSVector2 right) => new(left.X * right.X, left.Y * right.Y);
        public static DSVector2 operator /(DSVector2 left, DSVector2 right) => new(left.X / right.X, left.Y / right.Y);
        public static DSVector2 operator +(DSVector2 left, float right) => left + new DSVector2(right);
        public static DSVector2 operator -(DSVector2 left, float right) => left - new DSVector2(right);
        public static DSVector2 operator *(DSVector2 left, float right) => left * new DSVector2(right);
        public static DSVector2 operator /(DSVector2 left, float right) => left / new DSVector2(right);
        public static DSVector2 operator +(float right, DSVector2 left) => left + right;
        public static DSVector2 operator -(float right, DSVector2 left) => left - right;
        public static DSVector2 operator *(float right, DSVector2 left) => left * right;
        public static DSVector2 operator /(float right, DSVector2 left) => left / right;
        public static DSVector2 operator -(DSVector2 vec) => -1 * vec;

        public static float Dot(DSVector2 left, DSVector2 right) => left.X * right.X + left.Y * right.Y;
        public float Dot(DSVector2 vector) => X * vector.X + Y * vector.Y;
        public static DSVector2 Normalize(DSVector2 vector) => new DSVector2(vector.X, vector.Y) / vector.Length;
        public DSVector2 Normalize() => this / Length;

        public override string ToString() => $"X: {X},Y: {Y}";
    }
    public class DSVector3
    {
        public float X { get; set; } = 0;
        public float Y { get; set; } = 0;
        public float Z { get; set; } = 0;
        public float Length { get { return MathF.Sqrt(X * X + Y * Y + Z * Z); } }

        public DSVector3() { }
        public DSVector3(float value) : this(value, value, value) { }
        public DSVector3(float[] value) : this(value[0], value[1], value[2]) { }
        public DSVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static implicit operator Vector3(DSVector3 vector) => new(vector.X, vector.Y, vector.Z);
        public static implicit operator DSVector3(Vector3 vector) => new(vector.X, vector.Y, vector.Z);

        public static DSVector3 operator +(DSVector3 left, DSVector3 right) => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        public static DSVector3 operator -(DSVector3 left, DSVector3 right) => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        public static DSVector3 operator *(DSVector3 left, DSVector3 right) => new(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
        public static DSVector3 operator /(DSVector3 left, DSVector3 right) => new(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
        public static DSVector3 operator +(DSVector3 left, float right) => left + new DSVector3(right);
        public static DSVector3 operator -(DSVector3 left, float right) => left - new DSVector3(right);
        public static DSVector3 operator *(DSVector3 left, float right) => left * new DSVector3(right);
        public static DSVector3 operator /(DSVector3 left, float right) => left / new DSVector3(right);
        public static DSVector3 operator +(float right, DSVector3 left) => left + right;
        public static DSVector3 operator -(float right, DSVector3 left) => left - right;
        public static DSVector3 operator *(float right, DSVector3 left) => left * right;
        public static DSVector3 operator /(float right, DSVector3 left) => left / right;
        public static DSVector3 operator -(DSVector3 vec) => -1 * vec;

        public static float Dot(DSVector3 left, DSVector3 right) => left.X * right.X + left.Y * right.Y + left.Z * right.Z;
        public float Dot(DSVector3 vector) => X * vector.X + Y * vector.Y+ Z * vector.Z;
        public static DSVector3 Cross(DSVector3 left, DSVector3 right) => new((left.Y * right.Z) - (left.Z * right.Y), (left.Z * right.X) - (left.X * right.Z), (left.X * right.Y) - (left.Y * right.X));
        public DSVector3 Cross(DSVector3 vector) => new((Y * vector.Z) - (Z * vector.Y), (Z * vector.X) - (X * vector.Z), (X * vector.Y) - (Y * vector.X));
        public static DSVector3 Normalize(DSVector3 vector) => vector / vector.Length;
        public DSVector3 Normalize() => this / Length;

        public override string ToString() => $"X: {X},Y: {Y},Z: {Z}";
    }
    public class DSQuaternion
    {
        public float X { get; set; } = 0;
        public float Y { get; set; } = 0;
        public float Z { get; set; } = 0;
        public float W { get; set; } = 1;

        public DSQuaternion() { }
        public DSQuaternion(float value) : this (value, value, value, value) { }
        public DSQuaternion(DSVector3 vec,float w):this(vec.X,vec.Y,vec.Z,w) { }
        public DSQuaternion(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public static implicit operator Quaternion(DSQuaternion vector) => new(vector.X, vector.Y, vector.Z, vector.W);
        public static implicit operator DSQuaternion(Quaternion vector) => new(vector.X, vector.Y, vector.Z, vector.W);

        public static float Dot(DSQuaternion left, DSQuaternion right) => left.X * right.X + left.Y * right.Y + left.Z * right.Z + left.W * right.W;
        public float Dot(DSQuaternion quat) => X * quat.X + Y * quat.Y + Z * quat.Z + W * quat.W;
        public static DSQuaternion Inverse(DSQuaternion value)
        {
            float ls = value.X * value.X + value.Y * value.Y + value.Z * value.Z + value.W * value.W;
            float invNorm = 1.0f / ls;

            return new(-value.X * invNorm, -value.Y * invNorm, -value.Z * invNorm, value.W * invNorm);
        }
        public DSQuaternion Inverse() => Inverse(this);
        public static DSQuaternion operator *(DSQuaternion left, DSQuaternion right)
        {
            float cx = left.Y * right.Z - left.Z * right.Y;
            float cy = left.Z * right.X - left.X * right.Z;
            float cz = left.X * right.Y - left.Y * right.X;

            float dot = left.X * right.X + left.Y * right.Y + left.Z * right.Z;

            return new(
                left.X * right.W + right.X * left.W + cx,
                left.Y * right.W + right.Y * left.W + cy,
                left.Z * right.W + right.Z * left.W + cz,
                left.W * right.W - dot);

        }

        public static DSVector3 operator *(DSQuaternion quat, DSVector3 vec)
        {
            DSQuaternion vecQuat = new(vec, 0);
            vecQuat = quat * vecQuat * quat.Inverse();
            return new DSVector3(vecQuat.X, vecQuat.Y, vecQuat.Z);
        }
        public static DSVector3 operator *(DSVector3 vec, DSQuaternion quat) => quat * vec;

        public override string ToString() => $"X: {X},Y: {Y},Z: {Z},W: {W}";
    }
    public class DSTouch
    {
        public int Id { get; set; } = 0;
        public bool Pressed { get; set; } = false;
        public DSVector2 Pos { get; set; } = new();

        public override string ToString() => $"Id:{Id},P:{Pressed},{Pos}";
    }
    public struct DSInputReport
    {
        public float Battery { get; set; } = 0;
        public bool Usb { get; set; } = false;
        public float[] Axis { get; } = new float[6];
        public float[] Sliders { get; } = Array.Empty<float>();
        public bool[] Buttons { get; } = new bool[14];
        public DSPov[] Povs { get; } = new DSPov[1] { new() };
        public DSVector3[] SixAxis { get; } = new DSVector3[4] { new(), new(), new(), new() };
        public DSQuaternion DeltaRotation { get; set; } = new();
        public DSQuaternion Rotation { get; set; } = new();

        public DSTouch[] Touch { get; } = new DSTouch[2] { new(), new() };
        public DSVector2 TouchPadSize { get; set;  } = new();

        public float deltaTime = 0;

        public DSInputReport() { }
        public DSInputReport(byte axis = 6, byte sliders = 0, byte buttons = 14, byte povs = 1, byte touchs = 2)
        {
            Axis = new float[axis];
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
        public void SetAxis(float[] axis)
        {
            int length = Math.Min(Axis.Length, axis.Length);

            for (int i = 0; i < length; i++)
                Axis[i] = axis[i];
        }
        public void SetSliders(float[] sliders)
        {
            int length = Math.Min(Sliders.Length, sliders.Length);

            for (int i = 0; i < length; i++)
                Sliders[i] = sliders[i];
        }
        public void SetButtons(bool[] buttons) => SetButtons(buttons, 0, buttons.Length);
        /*{
            int length = Math.Min(Buttons.Length,buttons.Length);

            for(int i = 0;i<length;i++)
                Buttons[i] = buttons[i];
        }*/
        public void SetButtons(bool[] buttons,int offset,int length)
        {
            int runLength = Math.Min(Buttons.Length-offset, length);

            for (int i = offset; i < runLength; i++)
                Buttons[i] = buttons[i];
        }
        public void SetPovs(DSPov[] povs)
        {
            int length = Math.Min(Povs.Length, povs.Length);

            for (int i = 0; i < length; i++)
                Povs[i] = povs[i];
        }
        public void SetTouchPads(DSTouch[] touch)
        {
            int length = Math.Min(Touch.Length, touch.Length);

            for (int i = 0; i < length; i++)
                Touch[i] = touch[i];
        }

        #region Axis
        public float LX { get { return Axis[0]; } set { Axis[0] = value; } }
        public float LY { get { return Axis[1]; } set { Axis[1] = value; } }
        public float RX { get { return Axis[2]; } set { Axis[2] = value; } }
        public float RY { get { return Axis[3]; } set { Axis[3] = value; } }
        public float LTrigger { get { return Axis[4]; } set { Axis[4] = value; } }
        public float RTrigger { get { return Axis[5]; } set { Axis[5] = value; } }
        #endregion Axis

        #region POV1
        public DSPov Pov { get => Povs[0]; set => Povs[0] = value; }
        public bool Up { get { return Povs[0].Up; } set { Povs[0].Up = value; } }
        public bool Right { get { return Povs[0].Right; } set { Povs[0].Right = value; } }
        public bool Down { get { return Povs[0].Down; } set { Povs[0].Down = value; } }
        public bool Left { get { return Povs[0].Left; } set { Povs[0].Left = value; } }
        #endregion POV1

        #region DS4Layout
        public bool Square { get { return Buttons[0]; } set { Buttons[0] = value; } }
        public bool Cross { get { return Buttons[1]; } set { Buttons[1] = value; } }
        public bool Circle { get { return Buttons[2]; } set { Buttons[2] = value; } }
        public bool Triangle { get { return Buttons[3]; } set { Buttons[3] = value; } }
        public bool L1 { get { return Buttons[4]; } set { Buttons[4] = value; } }
        public bool R1 { get { return Buttons[5]; } set { Buttons[5] = value; } }
        public bool L2 { get { return Buttons[6]; } set { Buttons[6] = value; } }
        public bool R2 { get { return Buttons[7]; } set { Buttons[7] = value; } }
        public bool Share { get { return Buttons[8]; } set { Buttons[8] = value; } }
        public bool Options { get { return Buttons[9]; } set { Buttons[9] = value; } }
        public bool L3 { get { return Buttons[10]; } set { Buttons[10] = value; } }
        public bool R3 { get { return Buttons[11]; } set { Buttons[11] = value; } }
        public bool PS { get { return Buttons[12]; } set { Buttons[12] = value; } }
        public bool TouchClick { get { return Buttons[13]; } set { Buttons[13] = value; } }
        #endregion DS4Layout

        #region XboxLayout
        public bool X { get { return Buttons[0]; } set { Buttons[0] = value; } }
        public bool A { get { return Buttons[1]; } set { Buttons[1] = value; } }
        public bool B { get { return Buttons[2]; } set { Buttons[2] = value; } }
        public bool Y { get { return Buttons[3]; } set { Buttons[3] = value; } }
        public bool LButton { get { return Buttons[4]; } set { Buttons[4] = value; } }
        public bool RButton { get { return Buttons[5]; } set { Buttons[5] = value; } }
        public bool Back { get { return Buttons[8]; } set { Buttons[8] = value; } }
        public bool Start { get { return Buttons[9]; } set { Buttons[9] = value; } }
        public bool LThumb { get { return Buttons[10]; } set { Buttons[10] = value; } }
        public bool RThumb { get { return Buttons[11]; } set { Buttons[11] = value; } }
        public bool Guide { get { return Buttons[12]; } set { Buttons[12] = value; } }
        #endregion XboxLayout

        #region SixAxis
        public DSVector3 RawAccel { get { return SixAxis[0]; } set { SixAxis[0] = value; } }
        public DSVector3 Gyro { get { return SixAxis[1]; } set { SixAxis[1] = value; } }
        public DSVector3 Grav { get { return SixAxis[2]; } set { SixAxis[2] = value; } }
        public DSVector3 Accel { get { return SixAxis[3]; } set { SixAxis[3] = value; } }
        #endregion SixAxis
    }
    public struct DSOutputReport
    {
        public float[] Rumble { get; set; } = new float[6];
        public DSLight Led { get; set; } = new();
        public float[] ExtLeds { get; set; } = new float[6];

        public DSOutputReport() { }

        #region DS4Layout
        public float Right { get { return Rumble[0]; } set { Rumble[0] = value; } }
        public float Left { get { return Rumble[1]; } set { Rumble[1] = value; } }
        public float Weak { get { return Rumble[0]; } set { Rumble[0] = value; } }
        public float Strong { get { return Rumble[1]; } set { Rumble[1] = value; } }
        public float Red { get { return Led.Red; } set { Led.Red = value; } }
        public float Green { get { return Led.Green; } set { Led.Green = value; } }
        public float Blue { get { return Led.Blue; } set { Led.Blue = value; } }
        public float OnTime { get { return Led.OnTime; } set { Led.OnTime = value; } }
        public float OffTime { get { return Led.OffTime; } set { Led.OffTime = value; } }
        #endregion DS4Layout
    }
}
