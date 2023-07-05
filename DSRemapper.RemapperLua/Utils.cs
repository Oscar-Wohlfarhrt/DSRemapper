using DSRemapper.Core;
using DSRemapper.DSMath;
using DSRemapper.SixAxis;
using DSRemapper.Types;
using MoonSharp.Interpreter;

using MoonSharp.Interpreter.Serialization.Json;

namespace DSRemapper.RemapperLua
{
    public static class Utils
    {
        public const float MinAxis = -1;
        public const float CenterAxis = 0;
        public const float MaxAxis = 1;
        public const float MinTrigger = 0;
        public const float MaxTrigger = 1;
        private const float defaulLedIntensity = 0.125f;

        public static DSInputReport CreateInputReport() => new();
        public static DSOutputReport CreateOutputReport() => new() { Red = 0.4f * defaulLedIntensity, Green = 0.8f * defaulLedIntensity, Blue = 1f * defaulLedIntensity };
        public static SimpleSignalFilter CreateSSF() => new();
        public static ExpMovingAverage CreateAverange() => new();
        public static ExpMovingAverageVector3 CreateAverangeVec3() => new();
        public static DSVector2 CreateVector2(float val = 0) => new(val);
        public static DSVector2 CreateVector2(float x,float y) => new(x,y);
        public static DSVector3 CreateVector3(float val = 0) => new(val);
        public static DSVector3 CreateVector3(float x, float y,float z) => new(x,y,z);
        public static DSQuaternion CreateQuaternion() => new();

        public static float Deadzone(this float value, float deadzone)
        {
            float valueAbs = value.Abs();
            return valueAbs < deadzone ? 0 : (valueAbs.Remap(deadzone, 1, 0, 1) * value.Sign());
        }
        public static float NoLinAxis(this float value, float noLinear)
        {
            noLinear = noLinear.Clamp(0,1);
            return noLinear*MathF.Pow(value,3)+(1-noLinear)*value;
        }
        public static float LinVecComp(this float value) => (2f / MathF.PI) * MathF.Asin(value);
        public static float Abs(this float value) => Math.Abs(value);
        public static float Remap(this float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
        public static float Clamp(this float value, float min, float max)
        {
            return Math.Clamp(value, min, max);
        }
        public static float ClampAxis(this float value) => value.Clamp(MinAxis, MaxAxis);
        public static float ClampTrigger(this float value) => value.Clamp(MinTrigger, MaxTrigger);

        public static float Sign(this float value)
        {
            return Math.Sign(value);
        }
        public static float SignPow(this float value, float pow)
        {
            return (float)Math.Pow(value, pow) * Math.Sign(value);
        }
        public static float Bend(this float value, float bend)
        {
            return (1 - bend) * (float)Math.Pow(value, 3) + bend * value;
        }

        public static void SaveTable(string path, Table table)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string json = JsonTableConverter.TableToJson(table);
            if (Path.IsPathRooted(path))
                File.WriteAllText(path, json);
            else
                File.WriteAllText(Path.Combine(DSPaths.ConfigPath,path), json);
        }
        public static Table LoadTable(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string json = "";
            if (Path.IsPathFullyQualified(path))
                json = File.ReadAllText(path);
            else
                json = File.ReadAllText(Path.Combine(DSPaths.ConfigPath, path));

            return JsonTableConverter.JsonToTable(json);
        }
    }
}
