using DSRemapper.Core;
using DSRemapper.DSMath;
using DSRemapper.SixAxis;
using DSRemapper.Types;
using MoonSharp.Interpreter;
using System.Numerics;
using static System.Net.Mime.MediaTypeNames;

namespace DSRemapper.RemapperLua
{
    [Remapper("lua")]
    public class LuaInterpreter : IDSRemapper
    {
        static LuaInterpreter()
        {
            UserData.RegisterType<DSInputReport>(InteropAccessMode.BackgroundOptimized);
            UserData.RegisterType<DSTouch>(InteropAccessMode.BackgroundOptimized);
            UserData.RegisterType<DSTouch[]>(InteropAccessMode.BackgroundOptimized);
            UserData.RegisterType<DSLight>(InteropAccessMode.BackgroundOptimized);
            UserData.RegisterType<DSPov>(InteropAccessMode.BackgroundOptimized);
            UserData.RegisterType<DSPov[]>(InteropAccessMode.BackgroundOptimized);
            UserData.RegisterType<DSOutputReport>(InteropAccessMode.BackgroundOptimized);

            //UserData.RegisterType(typeof(Utils), InteropAccessMode.BackgroundOptimized);
            //UserData.RegisterExtensionType(typeof(Utils), InteropAccessMode.BackgroundOptimized);

            UserData.RegisterType<IDSOutputController>(InteropAccessMode.BackgroundOptimized);

            /*UserData.RegisterType<MKOutput>(InteropAccessMode.BackgroundOptimized);
            UserData.RegisterType<VirtualKeyShort>(InteropAccessMode.BackgroundOptimized);
            UserData.RegisterType<ScanCodeShort>(InteropAccessMode.BackgroundOptimized);
            UserData.RegisterType<MouseButton>(InteropAccessMode.BackgroundOptimized);*/

            UserData.RegisterType<Vector2>(InteropAccessMode.BackgroundOptimized);
            UserData.RegisterType<Vector3>(InteropAccessMode.BackgroundOptimized);
            UserData.RegisterType<Quaternion>(InteropAccessMode.BackgroundOptimized);
            UserData.RegisterType<DSVector2>(InteropAccessMode.BackgroundOptimized);
            UserData.RegisterType<DSVector3>(InteropAccessMode.BackgroundOptimized);
            UserData.RegisterType<DSQuaternion>(InteropAccessMode.BackgroundOptimized);

            UserData.RegisterType<SimpleSignalFilter>(InteropAccessMode.BackgroundOptimized);
            UserData.RegisterType<ExpMovingAverage>(InteropAccessMode.BackgroundOptimized);
            UserData.RegisterType<ExpMovingAverageVector3>(InteropAccessMode.BackgroundOptimized);

            UserData.RegisterType<bool[]>(InteropAccessMode.BackgroundOptimized);
            UserData.RegisterType<float[]>(InteropAccessMode.BackgroundOptimized);
        }
        Script script = new Script();

        public event RemapperEventArgs? OnLog;

        public LuaInterpreter()
        {

        }
        public void SetScript(string file)
        {
            try
            {
                script.Globals["ConsoleLog"] = (Action<string>)ConsoleLog;
                script.DoFile(file);
            }
            catch (Exception e)
            {
                OnLog?.Invoke(RemapperEventType.Error, e.Message);
            }
        }
        public DSOutputReport Remap(DSInputReport report)
        {
            return new();
        }
        private void ConsoleLog(string text)
        {
            OnLog?.Invoke(RemapperEventType.DeviceConsole,text);
        }
        public void Dispose()
        {

        }
    }
}