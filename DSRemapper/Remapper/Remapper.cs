using DSRemapper.Configs;
using DSRemapper.DSInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DSRemapper.Remapper
{
    public class RemapperScriptEventArgs : EventArgs
    {
        public string id { get; set; } = "";
        public string message { get; set; } = "";
    }
    public class RemapperReportEventArgs : EventArgs
    {
        public string id { get; set; } = "";
        public DSInputReport report { get; set; } = new DSInputReport();
    }

    public interface IRemapper : IDisposable
    {
        public string ControllerId { get; }
        public IDSInputController Controller { get; }
        public string LastProfile { get; }
        public event EventHandler<RemapperScriptEventArgs>? OnError;
        public event EventHandler<RemapperScriptEventArgs>? OnLog;
        public event EventHandler<RemapperReportEventArgs>? OnReportUpdate;

        public void LoadScript(string profileName);
        public void ReloadScript();
        public void RemapController();
        public DSInputReport GetInputReport();
        public void Connect();
        public void DisconnectEmulatedControllers();
    }

    public class RemapperCore
    {
        public List<IRemapper> controlRemapperList = new List<IRemapper>();

        public event EventHandler<RemapperScriptEventArgs>? OnError;
        public event EventHandler<RemapperScriptEventArgs>? OnLog;
        private RemapperScriptEventArgs loadDefault = new RemapperScriptEventArgs();
        public event EventHandler<RemapperScriptEventArgs>? LoadDefaultProfile;
        public event EventHandler<RemapperReportEventArgs>? OnReportUpdate;

        public void SetControllers(List<IDSInputController> controllers)
        {
            List<IRemapper> removeList = new List<IRemapper>();
            foreach (var ctrl in controlRemapperList)
            {
                if (!controllers.Exists((c) => { return c.Id == ctrl.ControllerId; }))
                    removeList.Add(ctrl);
            }

            foreach (var ctrl in removeList)
                RemoveController(ctrl.ControllerId);

            foreach (var ctrl in controllers)
                AddController(ctrl);
        }
        public void AddController(IDSInputController controller, string? profileName = null)
        {
            if (!controlRemapperList.Exists((c) => { return c.ControllerId == controller.Id; }))
            {
                IRemapper ctrlRemapper;

                if (profileName == null)
                    profileName = ProfileManager.GetLastProfile(controller.Id);

                switch (Path.GetExtension(profileName))
                {
                    case ".cs":
                        Console.WriteLine("C# Profile");
                        ctrlRemapper = new CSRemapper(controller, OnError, OnLog, OnReportUpdate);
                        break;
                    default:
                        Console.WriteLine("Lua Profile");
                        ctrlRemapper = new LuaRemapper(controller, OnError, OnLog, OnReportUpdate);
                        break;
                }

                ctrlRemapper.LoadScript(profileName);

                loadDefault.id = controller.Id;
                loadDefault.message = ctrlRemapper.LastProfile;
                LoadDefaultProfile?.Invoke(this, loadDefault);

                ctrlRemapper.Connect();

                controlRemapperList.Add(ctrlRemapper);
            }
        }
        public void RemoveController(string controllerId)
        {
            IRemapper? ctrlRemapper = controlRemapperList.Find((c) => { return c.ControllerId == controllerId; });

            if (ctrlRemapper != null)
            {
                controlRemapperList.Remove(ctrlRemapper);
                ctrlRemapper.DisconnectEmulatedControllers();
                ctrlRemapper.Controller.Dispose();
            }
        }

        public IRemapper GetControlRemapper(string controllerId)
        {
            int index = controlRemapperList.FindIndex((c) => { return c.ControllerId == controllerId; });

            if (index==-1)
                throw new Exception("Controller not found");

            return controlRemapperList[index];
        }
        public int GetControlRemapperIndex(string controllerId)
        {
            int index = controlRemapperList.FindIndex((c) => { return c.ControllerId == controllerId; });

            if (index == -1)
                throw new Exception("Controller not found");

            return index;
        }
        public void SetControllerProfile(string controllerId, string profileName)
        {
            int index = GetControlRemapperIndex(controllerId);

            controlRemapperList[index].DisconnectEmulatedControllers();

            switch (Path.GetExtension(profileName))
            {
                case ".cs":
                    Console.WriteLine("C# Profile");
                    if(controlRemapperList[index] is not CSRemapper)
                        controlRemapperList[index] = new CSRemapper(controlRemapperList[index].Controller, OnError, OnLog, OnReportUpdate);
                    break;
                default:
                    Console.WriteLine("Lua Profile");
                    if (controlRemapperList[index] is not LuaRemapper)
                        controlRemapperList[index] = new LuaRemapper(controlRemapperList[index].Controller, OnError, OnLog, OnReportUpdate);
                    break;
            }

            controlRemapperList[index].LoadScript(profileName);
        }
        public DSInputReport GetControllerInputReport(string controllerId)
        {
            return GetControlRemapper(controllerId).GetInputReport();
        }

        public void RemapAll()
        {
            foreach (var control in controlRemapperList.ToArray())
            {
                control.RemapController();
            }
        }
    }
}
