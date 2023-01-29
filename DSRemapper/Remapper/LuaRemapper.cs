using DSRemapper.Configs;
using DSRemapper.DSInput;
using MoonSharp.Interpreter;
using DSRemapper.DSOutput;
using DSRemapper.ControllerOutput;
using static DSRemapper.ControllerOutput.SendInputApi;
using System.Numerics;

namespace DSRemapper.Remapper
{
    public class LuaRemapper
    {
        public string ControllerId { get { return Controller.Id; } }
        public IDSInputController Controller { get; private set; }
        private Script script = new();
        private Closure? remapFunction = null;
        private Task? RemapTask=null;
        public bool RemapCompleted { get => RemapTask==null || RemapTask.IsCompleted; }

        readonly private RemapperScriptEventArgs eventArgs = new();
        readonly private RemapperReportEventArgs reportArgs = new();
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
                script.Globals["CreateDS4"] = (Func<IDSOutputController>)emuCtrls.CreateDS4Controller;
                script.Globals["CreateXbox"] = (Func<IDSOutputController>)emuCtrls.CreateXboxController;
                script.Globals["CreateVJoy"] = (Func<uint,uint,IDSOutputController>)emuCtrls.CreateVJoyController;
                script.Globals["ConsoleLog"] = (Action<string>)ConsoleLog;

                script.Globals["Utils"] = typeof(Utils);
                script.Globals["MKOut"] = new MKOutput();
                script.Globals["Keys"] = new VirtualKeyShort();
                script.Globals["Scans"] = new ScanCodeShort();
                script.Globals["MButs"] = new MouseButton();
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
            RemapTask=Task.Factory.StartNew(() =>
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
                    try
                    {
                        Controller.SendOutputReport((DSOutputReport)script.Globals["inputFB"]);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                catch (Exception e)
                {
                    if (eventArgs.message != e.Message)
                    {
                        eventArgs.message = e.Message;
                        OnError?.Invoke(this, eventArgs);
                    }
                }
            });
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