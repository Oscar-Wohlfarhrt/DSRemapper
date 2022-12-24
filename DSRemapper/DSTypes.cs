using System.Numerics;

namespace DSRemapper
{
    public class DSPov
    {
        private float angle = 0;
        public float Angle { get { return angle; } set { angle = value < 0 ? -1 : Math.Clamp(value, 0, 360); } }
        public bool Up { get; set; } = false;
        public bool Left { get; set; } = false;
        public bool Down { get; set; } = false;
        public bool Right { get; set; } = false;

        internal void SetDSPov(byte pov)
        {
            Angle = pov != 8 ? pov * 360f / 8f : -1;

            /*
            byte dPad = (byte)(rawReport[continuosOffset] & 0x0F);
            int bitSwap = dPad / 2;
            dPad = (byte)(((1 << bitSwap) | ((dPad % 2 << (bitSwap + 1)) % 15)) & 0b1111);*/

            int dPad = pov;
            if (Angle >= 0)
            {
                int bitSwap = dPad / 2;
                dPad = (((1 << bitSwap) | ((dPad % 2 << (bitSwap + 1)) % 15)) & 0b1111);
            }
            else
                dPad = 0;

            Up = Convert.ToBoolean(dPad & (1 << 0));
            Right = Convert.ToBoolean(dPad & (1 << 1));
            Down = Convert.ToBoolean(dPad & (1 << 2));
            Left = Convert.ToBoolean(dPad & (1 << 3));
        }

        public void CalculateAngle()
        {
            if (Up && !Left && !Down && !Right)
                Angle = 0;
            else if (Up && Left && !Down && !Right)
                Angle = 45;
            else if (!Up && Left && !Down && !Right)
                Angle = 90;
            else if (!Up && Left && Down && !Right)
                Angle = 135;
            else if (!Up && !Left && !Down && !Right)
                Angle = 180;
            else if (!Up && !Left && !Down && !Right)
                Angle = 225;
            else if (!Up && !Left && !Down && !Right)
                Angle = 270;
            else if (!Up && !Left && !Down && !Right)
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

            Up = Convert.ToBoolean(dPad & (1 << 0));
            Right = Convert.ToBoolean(dPad & (1 << 1));
            Down = Convert.ToBoolean(dPad & (1 << 2));
            Left = Convert.ToBoolean(dPad & (1 << 3));
        }
    }
    public class DSLight
    {
        private readonly float[] led = new float[3] { 0f, 0f, 0f };
        private readonly float[] OnOff = new float[2] { 0f, 0f };
        public float Red { get { return led[0]; } set { led[0] = Math.Clamp(value, 0, 1); } }
        public float Green { get { return led[1]; } set { led[1] = Math.Clamp(value, 0, 1); } }
        public float Blue { get { return led[2]; } set { led[2] = Math.Clamp(value, 0, 1); } }
        public float OnTime { get { return OnOff[0]; } set { OnOff[0] = Math.Clamp(value, 0, 1); } }
        public float OffTime { get { return OnOff[1]; } set { OnOff[1] = Math.Clamp(value, 0, 1); } }

        public DSLight() { }
        public DSLight(float red, float green, float blue, float intensity = 1)
        {
            Red = red * intensity;
            Green = green * intensity;
            Blue = blue * intensity;
        }

        public static DSLight operator *(DSLight light, float intensity)
        {
            light.Red *= intensity;
            light.Green *= intensity;
            light.Blue *= intensity;

            return light;
        }
        public static DSLight operator *(float intensity, DSLight light) => light * intensity;
        public static DSLight operator +(DSLight light, float add)
        {
            light.Red += add;
            light.Green += add;
            light.Blue += add;

            return light;
        }
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
        public DSVector2 Normalize() => new DSVector2(X, Y) / Length;
    }
    public class DSVector3
    {
        public float X { get; set; } = 0;
        public float Y { get; set; } = 0;
        public float Z { get; set; } = 0;
        public float Length { get { return MathF.Sqrt(X * X + Y * Y + Z * Z); } }

        public DSVector3() { }
        public DSVector3(float value) : this(value, value, value) { }
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
    }
}
