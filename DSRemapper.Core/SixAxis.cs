using DSRemapper.DSMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DSRemapper.SixAxis
{
    public struct SimpleSignalFilter
    {
        private DSVector3 sample1, y;

        public SimpleSignalFilter()
        {
            sample1 = y = new();
        }
        public static SimpleSignalFilter CreateSSF()
        {
            return new SimpleSignalFilter();
        }
        public DSVector3 LowPass(DSVector3 sample, float x0Strength)
        {
            y = (1 - x0Strength) * y + x0Strength * sample;

            return y;
        }
        public DSVector3 LowPass(DSVector3 sample, float x0Strength, float x1Strength)
        {
            y = (1 - x0Strength - x1Strength) * y + x0Strength * sample + x1Strength * sample1;

            sample1 = sample;
            return y;
        }
    }
    public struct ExpMovingAverage
    {
        int n = 0;

        public float Mean { get; private set; } = 0;

        public ExpMovingAverage() { }

        public float Update(float newValue, int maxN = 0)
        {
            if (n == 0 || n < maxN)
                n++;

            Mean += (newValue - Mean) / n;
            return Mean;
        }
    }
    public struct ExpMovingAverageVector3
    {
        int n = 0;

        public DSVector3 Mean { get; private set; } = new();

        public ExpMovingAverageVector3() { }

        public DSVector3 Update(DSVector3 newValue, int maxN = 0)
        {
            if (n == 0 || n < maxN)
                n++;

            Mean += (newValue - Mean) / n;
            return Mean;
        }
    }
    public struct CustomMotionProcess
    {
        const float accelCorrection = 0.05f;

        private DateTime now = DateTime.Now, lastUpdate = DateTime.Now;
        public float DeltaTime { get; private set; } = 0;

        public DSQuaternion deltaRotation = Quaternion.Identity;
        public DSQuaternion rotation = Quaternion.Identity;
        public DSVector3 grav = new(0, -1, 0);
        public DSVector3 Accel = new();

        public CustomMotionProcess() { }

        public void Update(DSVector3 accel, DSVector3 gyro)
        {
            now = DateTime.Now;
            DeltaTime = (now - lastUpdate).Ticks / (float)TimeSpan.TicksPerSecond;
            lastUpdate = now;

            float angleSpeed = gyro.Length * MathF.PI / 180f;
            float angle = angleSpeed * DeltaTime;
            DSVector3 unitAccel = accel.Length > 0 ? accel.Normalize() : new();

            if (angle != 0)
                deltaRotation = Quaternion.Normalize(Quaternion.CreateFromAxisAngle(Vector3.Normalize(gyro), angle));

            grav = (DSQuaternion)Quaternion.Inverse(deltaRotation) * grav;

            grav = Vector3.Normalize((1 - accelCorrection) * grav + accelCorrection * -unitAccel);

            rotation *= deltaRotation;

            Accel = rotation * (accel + grav);
        }
    }
}
