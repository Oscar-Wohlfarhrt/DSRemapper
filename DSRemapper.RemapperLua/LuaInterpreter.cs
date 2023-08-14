using DSRemapper.Core;
using DSRemapper.DSMath;
using DSRemapper.SixAxis;
using DSRemapper.Types;
using MoonSharp.Interpreter;
using System.Diagnostics;
using System.Numerics;
using DSRemapper.MouseKeyboardOutput;

namespace DSRemapper.RemapperLua
{
    /// <summary>
    /// Remapper plugin based on lua scripts for remapping controllers
    /// </summary>
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

            UserData.RegisterType(typeof(Utils), InteropAccessMode.BackgroundOptimized);
            UserData.RegisterExtensionType(typeof(Utils), InteropAccessMode.BackgroundOptimized);

            UserData.RegisterType<IDSOutputController>(InteropAccessMode.BackgroundOptimized);
            UserData.RegisterType<DSOutput.DSOutput>(InteropAccessMode.BackgroundOptimized);
            UserData.RegisterType<MKOutput>(InteropAccessMode.BackgroundOptimized);
            UserData.RegisterType<VirtualKeyShort>(InteropAccessMode.BackgroundOptimized);
            UserData.RegisterType<ScanCodeShort>(InteropAccessMode.BackgroundOptimized);
            UserData.RegisterType<MouseButton>(InteropAccessMode.BackgroundOptimized);

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

        private Script script = new();
        private Closure? luaRemap = null;
        /// <inheritdoc/>
        public event RemapperEventArgs? OnLog;
        private string lastMessage = "";

        private readonly DSOutput.DSOutput emuControllers = new();

        private readonly Stopwatch sw = new();
        /// <summary>
        /// LuaInterpreter class constructor
        /// </summary>
        public LuaInterpreter()
        {

        }
        /// <inheritdoc/>
        public void SetScript(string file)
        {
            try
            {
                emuControllers.DisconnectAll();
                script=new Script();

                script.Globals["ConsoleLog"] = (Action<string>)ConsoleLog;
                script.Globals["Emulated"] = emuControllers;
                script.Globals["Utils"] = typeof(Utils);
                script.Globals["inputFB"] = Utils.CreateOutputReport();
                script.Globals["deltaTime"] = 0.0;

                script.Globals["MKOut"] = new MKOutput();
                script.Globals["Keys"] = new VirtualKeyShort();
                script.Globals["Scans"] = new ScanCodeShort();
                script.Globals["MButs"] = new MouseButton();

                script.DoFile(file);

                Closure? remapFunction = (Closure)script.Globals["Remap"];
                luaRemap = remapFunction;
                if (remapFunction == null)
                    OnLog?.Invoke(RemapperEventType.Warning, "No Remap function on the script");
            }
            catch (InterpreterException e)
            {
                luaRemap = null;
                OnLog?.Invoke(RemapperEventType.Error, e.DecoratedMessage);
            }
            catch (Exception e)
            {
                luaRemap = null;
                OnLog?.Invoke(RemapperEventType.Error, e.Message);
            }
        }
        /// <inheritdoc/>
        public DSOutputReport Remap(DSInputReport report)
        {
            DSOutputReport outReport = new();
            try
            {
                script.Globals["deltaTime"] = sw.Elapsed.TotalSeconds;
                sw.Restart();
                if (luaRemap != null)
                    script.Call(luaRemap,report);
                DSOutputReport? feedback = (DSOutputReport?)script.Globals["inputFB"];
                if(feedback!=null)
                    outReport = feedback;
            }
            catch (InterpreterException e)
            {
                luaRemap = null;
                OnLog?.Invoke(RemapperEventType.Error, e.DecoratedMessage);
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

            return outReport;
        }
        private void ConsoleLog(string text)
        {
            OnLog?.Invoke(RemapperEventType.DeviceConsole,text);
        }
        /// <inheritdoc/>
        public void Dispose()
        {
            emuControllers.DisconnectAll();
            GC.SuppressFinalize(this);
        }
    }
}