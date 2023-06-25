using DSRemapper.Core;
using DSRemapper.DSMath;
using DSRemapper.SixAxis;
using DSRemapper.Types;
using MoonSharp.Interpreter;
using System.Diagnostics;
using System.Numerics;

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
            UserData.RegisterType<DSOutput.DSOutput>(InteropAccessMode.BackgroundOptimized);
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
        Closure? luaRemap = null;

        public event RemapperEventArgs? OnLog;
        private string lastMessage = "";

        DSOutput.DSOutput emuControllers = new();

        Stopwatch sw = new();

        public LuaInterpreter()
        {

        }
        public void SetScript(string file)
        {
            try
            {
                emuControllers.DisconnectAll();
                script=new Script();

                script.Globals["ConsoleLog"] = (Action<string>)ConsoleLog;
                script.Globals["Emulated"] = emuControllers;
                script.Globals["deltaTime"] = 0.0;
                script.DoFile(file);

                Closure? remapFunction = (Closure)script.Globals["Remap"];
                luaRemap = remapFunction;
                if (remapFunction == null)
                    OnLog?.Invoke(RemapperEventType.Warning, "No Remap function on the script");
            }
            catch (Exception e)
            {
                luaRemap = null;
                OnLog?.Invoke(RemapperEventType.Error, e.Message);
            }
        }
        public DSOutputReport Remap(DSInputReport report)
        {
            try
            {
                script.Globals["deltaTime"] = sw.Elapsed.TotalSeconds;
                sw.Restart();
                if (luaRemap != null)
                    script.Call(luaRemap,report);
            }
            catch (Exception e)
            {
                if (lastMessage != e.Message)
                {
                    lastMessage = e.Message;
                    OnLog?.Invoke(RemapperEventType.Error, e.Message);
                }
                luaRemap = null;
            }

            return new();
        }
        private void ConsoleLog(string text)
        {
            OnLog?.Invoke(RemapperEventType.DeviceConsole,text);
        }
        public void Dispose()
        {
            emuControllers.DisconnectAll();
        }
    }
}