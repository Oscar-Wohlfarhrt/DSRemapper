using System.Numerics;

namespace DSRemapper.DSMath
{
    /// <summary>
    /// Represents a vector with two single-precision floating-point values.
    /// Used on DSRemapper SixAxis calculations
    /// </summary>
    public class DSVector2
    {
        /// <inheritdoc cref="Vector2.X"/>
        public float X { get; set; } = 0;
        /// <inheritdoc cref="Vector2.Y"/>
        public float Y { get; set; } = 0;
        /// <summary>
        /// Gets the length of the vector.
        /// </summary>
        public float Length { get { return MathF.Sqrt(X * X + Y * Y); } }

        /// <summary>
        /// Creates a new DSVector2 object whose two elements are 0.
        /// </summary>
        public DSVector2() { }
        /// <summary>
        /// Creates a new <see cref="System.Numerics.Vector2" /> object whose two elements have the same value.
        /// </summary>
        /// <param name="value">The value to assign to both elements.</param>
        public DSVector2(float value) : this(value, value) { }
        /// <summary>Creates a vector whose elements have the specified values.</summary>
        /// <param name="x">The value to assign to the <see cref="X" /> property.</param>
        /// <param name="y">The value to assign to the <see cref="Y" /> property.</param>
        public DSVector2(float x, float y)
        {
            X = x;
            Y = y;
        }
        /// <summary>
        /// Conversion from DSVector2 to System.Numerics.Vector2
        /// </summary>
        /// <param name="vector">A DSVector2 to convert to System.Numerics.Vector2</param>
        public static implicit operator Vector2(DSVector2 vector) => new(vector.X, vector.Y);
        /// <summary>
        /// Conversion from System.Numerics.Vector2 to DSVector2
        /// </summary>
        /// <param name="vector">A System.Numerics.Vector2 to convert to DSVector2</param>
        public static implicit operator DSVector2(Vector2 vector) => new(vector.X, vector.Y);

        /// <inheritdoc cref="Vector2.operator +"/>
        public static DSVector2 operator +(DSVector2 left, DSVector2 right) => new(left.X + right.X, left.Y + right.Y);
        /// <inheritdoc cref="Vector2.operator -"/>
        public static DSVector2 operator -(DSVector2 left, DSVector2 right) => new(left.X - right.X, left.Y - right.Y);
        /// <inheritdoc cref="Vector2.operator *(Vector2,Vector2)"/>
        public static DSVector2 operator *(DSVector2 left, DSVector2 right) => new(left.X * right.X, left.Y * right.Y);
        /// <inheritdoc cref="Vector2.operator /(Vector2,Vector2)"/>
        public static DSVector2 operator /(DSVector2 left, DSVector2 right) => new(left.X / right.X, left.Y / right.Y);
        /// <inheritdoc cref="Vector2.operator *(Vector2,float)"/>
        public static DSVector2 operator *(DSVector2 left, float right) => left * new DSVector2(right);
        /// <inheritdoc cref="Vector2.operator /(Vector2,float)"/>
        public static DSVector2 operator /(DSVector2 left, float right) => left / new DSVector2(right);
        /// <inheritdoc cref="operator *(DSVector2, float)"/>
        public static DSVector2 operator *(float right, DSVector2 left) => left * right;
        /// <inheritdoc cref="operator /(DSVector2, float)"/>
        public static DSVector2 operator /(float right, DSVector2 left) => left / right;

        /// <inheritdoc cref="Vector2.operator -(Vector2)"/>
        public static DSVector2 operator -(DSVector2 vec) => -1 * vec;
        /// <inheritdoc cref="Vector2.Dot(Vector2, Vector2)"/>
        /// <param name="left">The first vector</param>
        /// <param name="right">The second vector</param>
        public static float Dot(DSVector2 left, DSVector2 right) => left.X * right.X + left.Y * right.Y;
        /// <inheritdoc cref="Dot(DSVector2, DSVector2)"/>
        public float Dot(DSVector2 right) => X * right.X + Y * right.Y;
        /// <inheritdoc cref="Vector2.Normalize(Vector2)"/>
        /// <param name="vector">The vector to normalize.</param>
        public static DSVector2 Normalize(DSVector2 vector) => new DSVector2(vector.X, vector.Y) / vector.Length;
        /// <inheritdoc cref="Normalize(DSVector2)"/>
        public DSVector2 Normalize() => this / Length;
        /// <inheritdoc/>
        public override string ToString() => $"X: {X},Y: {Y}";
    }
    /// <summary>
    /// Represents a vector with three single-precision floating-point values.
    /// Used on DSRemapper SixAxis calculations
    /// </summary>
    public class DSVector3
    {
        /// <inheritdoc cref="Vector3.X"/>
        public float X { get; set; } = 0;
        /// <inheritdoc cref="Vector3.Y"/>
        public float Y { get; set; } = 0;
        /// <inheritdoc cref="Vector3.Z"/>
        public float Z { get; set; } = 0;
        /// <summary>
        /// Gets the length of the vector.
        /// </summary>
        public float Length { get { return MathF.Sqrt(X * X + Y * Y + Z * Z); } }

        /// <summary>
        /// Creates a new DSVector3 object whose three elements are 0.
        /// </summary>
        public DSVector3() { }
        /// <summary>
        /// Creates a new DSVector3 object whose three elements have the same value.
        /// </summary>
        /// <param name="value">The value to assign to all three elements.</param>
        public DSVector3(float value) : this(value, value, value) { }
        /// <summary>
        /// Creates a vector whose elements have the specified values.
        /// </summary>
        /// <param name="x">The value to assign to the <see cref="X" /> property.</param>
        /// <param name="y">The value to assign to the <see cref="Y" /> property.</param>
        /// <param name="z">The value to assign to the <see cref="Z" /> property.</param>
        public DSVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        /// <summary>
        /// Conversion from DSVector3 to System.Numerics.Vector3
        /// </summary>
        /// <param name="vector">A DSVector3 to convert to System.Numerics.Vector3</param>
        public static implicit operator Vector3(DSVector3 vector) => new(vector.X, vector.Y, vector.Z);
        /// <summary>
        /// Conversion from System.Numerics.Vector3 to DSVector3
        /// </summary>
        /// <param name="vector">A System.Numerics.Vector3 to convert to DSVector3</param>
        public static implicit operator DSVector3(Vector3 vector) => new(vector.X, vector.Y, vector.Z);
        /// <inheritdoc cref="Vector3.operator +"/>
        public static DSVector3 operator +(DSVector3 left, DSVector3 right) => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        /// <inheritdoc cref="Vector3.operator -"/>
        public static DSVector3 operator -(DSVector3 left, DSVector3 right) => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        /// <inheritdoc cref="Vector3.operator *(Vector3,Vector3)"/>
        public static DSVector3 operator *(DSVector3 left, DSVector3 right) => new(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
        /// <inheritdoc cref="Vector3.operator /(Vector3,Vector3)"/>
        public static DSVector3 operator /(DSVector3 left, DSVector3 right) => new(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
        /// <inheritdoc cref="Vector3.operator *(Vector3,float)"/>
        public static DSVector3 operator *(DSVector3 left, float right) => left * new DSVector3(right);
        /// <inheritdoc cref="Vector3.operator /(Vector3,float)"/>
        public static DSVector3 operator /(DSVector3 left, float right) => left / new DSVector3(right);
        /// <inheritdoc cref="operator *(DSVector3,float)"/>
        public static DSVector3 operator *(float right, DSVector3 left) => left * right;
        /// <inheritdoc cref="operator /(DSVector3,float)"/>
        public static DSVector3 operator /(float right, DSVector3 left) => left / right;
        /// <inheritdoc cref="Vector3.operator -(Vector3)"/>
        public static DSVector3 operator -(DSVector3 vec) => -1 * vec;

        /// <inheritdoc cref="Vector3.Dot(Vector3, Vector3)"/>
        /// <param name="left">The first vector</param>
        /// <param name="right">The second vector</param>
        public static float Dot(DSVector3 left, DSVector3 right) => left.X * right.X + left.Y * right.Y + left.Z * right.Z;
        /// <inheritdoc cref="Dot(DSVector3, DSVector3)"/>
        public float Dot(DSVector3 right) => X * right.X + Y * right.Y + Z * right.Z;
        /// <inheritdoc cref="Vector3.Cross(Vector3, Vector3)"/>
        /// <param name="left">The first vector</param>
        /// <param name="right">The second vector</param>
        public static DSVector3 Cross(DSVector3 left, DSVector3 right) => new((left.Y * right.Z) - (left.Z * right.Y), (left.Z * right.X) - (left.X * right.Z), (left.X * right.Y) - (left.Y * right.X));
        /// <inheritdoc cref="Cross(DSVector3, DSVector3)"/>
        public DSVector3 Cross(DSVector3 right) => new((Y * right.Z) - (Z * right.Y), (Z * right.X) - (X * right.Z), (X * right.Y) - (Y * right.X));
        /// <inheritdoc cref="Vector3.Normalize(Vector3)"/>
        /// <param name="vector">The vector to normalize.</param>
        public static DSVector3 Normalize(DSVector3 vector) => vector / vector.Length;
        /// <inheritdoc cref="Normalize(DSVector3)"/>
        public DSVector3 Normalize() => this / Length;
        /// <inheritdoc/>
        public override string ToString() => $"X: {X},Y: {Y},Z: {Z}";
    }
    /// <summary>
    /// Represents a vector that is used to encode three-dimensional physical rotations.
    /// Used on DSRemapper SixAxis calculations
    /// </summary>
    public class DSQuaternion
    {
        /// <inheritdoc cref="Quaternion.X"/>
        public float X { get; set; } = 0;
        /// <inheritdoc cref="Quaternion.Y"/>
        public float Y { get; set; } = 0;
        /// <inheritdoc cref="Quaternion.Z"/>
        public float Z { get; set; } = 0;
        /// <inheritdoc cref="Quaternion.W"/>
        public float W { get; set; } = 1;
        /// <summary>
        /// Constructs an identity quaternion.
        /// </summary>
        public DSQuaternion() { }
        /// <summary>
        /// Creates a quaternion from the specified vector and rotation parts.
        /// </summary>
        /// <param name="vec">The vector part of the quaternion.</param>
        /// <param name="w">The rotation part of the quaternion.</param>
        public DSQuaternion(DSVector3 vec, float w) : this(vec.X, vec.Y, vec.Z, w) { }
        /// <summary>
        /// Constructs a quaternion from the specified components.
        /// </summary>
        /// <param name="x">The value to assign to the X component of the quaternion.</param>
        /// <param name="y">The value to assign to the Y component of the quaternion.</param>
        /// <param name="z">The value to assign to the Z component of the quaternion.</param>
        /// <param name="w">The value to assign to the W component of the quaternion.</param>
        public DSQuaternion(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        /// <summary>
        /// Conversion from DSQuaternion to System.Numerics.Quaternion
        /// </summary>
        /// <param name="vector">A DSQuaternion to convert to System.Numerics.Quaternion</param>
        public static implicit operator Quaternion(DSQuaternion vector) => new(vector.X, vector.Y, vector.Z, vector.W);
        /// <summary>
        /// Conversion from System.Numerics.Quaternion to DSQuaternion
        /// </summary>
        /// <param name="vector">A System.Numerics.Quaternion to convert to DSQuaternion</param>
        public static implicit operator DSQuaternion(Quaternion vector) => new(vector.X, vector.Y, vector.Z, vector.W);
        /// <inheritdoc cref="Quaternion.Dot(Quaternion, Quaternion)"/>
        public static float Dot(DSQuaternion left, DSQuaternion right) => left.X * right.X + left.Y * right.Y + left.Z * right.Z + left.W * right.W;
        /// <inheritdoc cref="Dot(DSQuaternion, DSQuaternion)"/>
        public float Dot(DSQuaternion right) => X * right.X + Y * right.Y + Z * right.Z + W * right.W;
        /// <inheritdoc cref="Quaternion.Inverse(Quaternion)"/>
        public static DSQuaternion Inverse(DSQuaternion value)
        {
            float ls = value.X * value.X + value.Y * value.Y + value.Z * value.Z + value.W * value.W;
            float invNorm = 1.0f / ls;

            return new(-value.X * invNorm, -value.Y * invNorm, -value.Z * invNorm, value.W * invNorm);
        }
        /// <inheritdoc cref="Inverse(DSQuaternion)"/>
        public DSQuaternion Inverse() => Inverse(this);
        /// <inheritdoc cref="Quaternion.operator *(Quaternion,Quaternion)"/>
        /// <param name="left">The first quaternion.</param>
        /// <param name="right">The second quaternion.</param>
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
        /// <summary>
        /// Rotates a DSVector3 using a DSQuaternion
        /// </summary>
        /// <param name="quat">The quaternion used for the rotation</param>
        /// <param name="vec">The 3D vector to rotate</param>
        /// <returns>A DSVector3 rotated by the rotation specified by the quaternion</returns>
        public static DSVector3 operator *(DSQuaternion quat, DSVector3 vec)
        {
            DSQuaternion vecQuat = new(vec, 0);
            vecQuat = quat * vecQuat * quat.Inverse();
            return new DSVector3(vecQuat.X, vecQuat.Y, vecQuat.Z);
        }
        /// <inheritdoc cref="operator *(DSQuaternion, DSVector3)"/>
        public static DSVector3 operator *(DSVector3 vec, DSQuaternion quat) => quat * vec;
        /// <inheritdoc/>
        public override string ToString() => $"X: {X},Y: {Y},Z: {Z},W: {W}";
    }
}
