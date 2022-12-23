using DSRemapper.Configs;
using DSRemapper.DSInput;
using MoonSharp.Interpreter;
using DSRemapper.DSOutput;
using DSRemapper.ControllerOutput;
using static DSRemapper.ControllerOutput.SendInputApi;
using System.Numerics;

namespace DSRemapper.Remapper
{
    public class LuaRemapper : IRemapper
    {
        public string ControllerId { get { return Controller.Id; } }
        public IDSInputController Controller { get; private set; }
        public Script script = new Script();
        public Closure? remapFunction = null;
        private RemapperScriptEventArgs eventArgs = new RemapperScriptEventArgs();
        private RemapperReportEventArgs reportArgs = new RemapperReportEventArgs();
        public event EventHandler<RemapperScriptEventArgs>? OnError;
        public event EventHandler<RemapperScriptEventArgs>? OnLog;
        public event EventHandler<RemapperReportEventArgs>? OnReportUpdate;
        public string LastProfile { get; private set; } = string.Empty;
        private DSOutputController emuCtrls = new DSOutputController();
        private DSInputReport lastInput = new DSInputReport();

        private DateTime now = DateTime.Now, lastUpdate = DateTime.Now;
        float deltaTime = 0;

        public LuaRemapper(IDSInputController controller, EventHandler<RemapperScriptEventArgs>? errHandler = null, EventHandler<RemapperScriptEventArgs>? logHandler = null, EventHandler<RemapperReportEventArgs>? batteryHandler = null)
        {
            this.Controller = controller;
            reportArgs.id = eventArgs.id = controller.Id;

            if (errHandler != null)
                OnError += errHandler;
            if (logHandler != null)
                OnLog += logHandler;
            if (batteryHandler != null)
                OnReportUpdate += batteryHandler;
        }
        public void LoadScript(string profileName)
        {
            ProfileManager.SetLastProfile(ControllerId, profileName);
            DisconnectEmulatedControllers();
            LastProfile = profileName;
            script = new Script();
            try
            {
                UserData.RegisterType<DSInputReport>(InteropAccessMode.BackgroundOptimized);
                UserData.RegisterType<DSTouch>(InteropAccessMode.BackgroundOptimized);
                UserData.RegisterType<DSOutputReport>(InteropAccessMode.BackgroundOptimized);
                UserData.RegisterExtensionType(typeof(Utils), InteropAccessMode.BackgroundOptimized);
                UserData.RegisterType<DSOutputController>(InteropAccessMode.BackgroundOptimized);
                UserData.RegisterType<IDSOutputController>(InteropAccessMode.BackgroundOptimized);
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
                UserData.RegisterType(typeof(Utils), InteropAccessMode.BackgroundOptimized);

                script.Globals["CreateDS4"] = (Func<IDSOutputController>)emuCtrls.CreateDS4Controller;
                script.Globals["CreateXbox"] = (Func<IDSOutputController>)emuCtrls.CreateXboxController;
                script.Globals["ConsoleLog"] = (Action<string>)ConsoleLog;

                script.Globals["Utils"] = typeof(Utils);
                script.Globals["MKOut"] = new MKOutput();
                script.Globals["Keys"] = new VirtualKeyShort();
                script.Globals["Scans"] = new ScanCodeShort();
                script.Globals["MButs"] = new MouseButton();
                script.Globals["Vector3"] = new Vector3();
                script.Globals["inputFB"] = Utils.CreateOutputReport();
                script.Globals["input"] = Utils.CreateInputReport();

                if (profileName != string.Empty)
                {
                    script.DoFile(ProfileManager.GetProfilePathByName(profileName));
                    remapFunction = (Closure)script.Globals["Remapper"];
                }
                else
                {
                    remapFunction = null;
                }
            }
            catch (Exception e)
            {
                string message = $"Profile Load Error:\n{e.Message}";
                Console.WriteLine(message);
                if (eventArgs.message != message)
                {
                    eventArgs.message = message;
                    OnError?.Invoke(this, eventArgs);
                }
            }
        }
        public void ReloadScript()
        {
            LoadScript(LastProfile);
        }
        public void RemapController()
        {
            now = DateTime.Now;
            deltaTime = (now - lastUpdate).Ticks / (float)TimeSpan.TicksPerSecond;
            lastUpdate = now;

            try
            {
                lastInput = Controller.GetInputReport();
                reportArgs.report = lastInput;
                OnReportUpdate?.Invoke(this, reportArgs);
                script.Globals["input"] = lastInput;
                script.Globals["deltaTime"] = deltaTime;
                if (remapFunction != null)
                {
                    script.Call(remapFunction);
                }
                Controller.SendOutputReport((DSOutputReport)script.Globals["inputFB"]);
            }
            catch (Exception e)
            {
                if (eventArgs.message != e.Message)
                {
                    eventArgs.message = e.Message;
                    OnError?.Invoke(this, eventArgs);
                }
            }
        }
        public DSInputReport GetInputReport()
        {
            try
            {
                return (DSInputReport)script.Globals["input"];
            }
            catch { }

            return new DSInputReport();
        }
        public void Connect() => Controller.Connect();
        public void DisconnectEmulatedControllers() => emuCtrls.DisconnectAll();

        public void ConsoleLog(string message)
        {
            eventArgs.message = message;
            OnLog?.Invoke(this, eventArgs);
        }

        public void Dispose()
        {
            Controller.Dispose();
        }
    }
}