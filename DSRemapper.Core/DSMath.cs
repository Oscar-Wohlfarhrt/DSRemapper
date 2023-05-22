using System.Numerics;

namespace DSRemapper.DSMath
{
    internal class DSMath
    {
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
        public float Dot(DSVector3 vector) => X * vector.X + Y * vector.Y + Z * vector.Z;
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
        public DSQuaternion(float value) : this(value, value, value, value) { }
        public DSQuaternion(DSVector3 vec, float w) : this(vec.X, vec.Y, vec.Z, w) { }
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
}
