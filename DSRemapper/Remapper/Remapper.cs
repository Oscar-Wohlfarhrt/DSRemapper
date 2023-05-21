using DSRemapper.Configs;
using DSRemapper.ControllerOutput;
using DSRemapper.DSInput;
using DSRemapper.DSOutput;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static DSRemapper.ControllerOutput.SendInputApi;

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

    public class RemapperCore
    {
        public List<LuaRemapper> controlRemapperList = new List<LuaRemapper>();

        public event EventHandler<RemapperScriptEventArgs>? OnError;
        public event EventHandler<RemapperScriptEventArgs>? OnLog;
        readonly private RemapperScriptEventArgs loadDefault = new();
        public event EventHandler<RemapperScriptEventArgs>? LoadDefaultProfile;
        public event EventHandler<RemapperReportEventArgs>? OnReportUpdate;

        public static CancellationTokenSource cts = new CancellationTokenSource();
        public static TaskFactory tFactory= new TaskFactory(cts.Token,TaskCreationOptions.None,TaskContinuationOptions.None,null);

        public RemapperCore()
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

            UserData.RegisterType<bool[]>(InteropAccessMode.BackgroundOptimized);
            UserData.RegisterType<float[]>(InteropAccessMode.BackgroundOptimized);
        }

        public void SetControllers(List<IDSInputController> controllers)
        {
            List<LuaRemapper> removeList = new List<LuaRemapper>();
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
                LuaRemapper ctrlRemapper;

                if (profileName == null)
                    profileName = ProfileManager.GetLastProfile(controller.Id);

                ctrlRemapper = new LuaRemapper(controller, OnError, OnLog, OnReportUpdate);

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
            LuaRemapper? ctrlRemapper = controlRemapperList.Find((c) => { return c.ControllerId == controllerId; });

            if (ctrlRemapper != null)
            {
                controlRemapperList.Remove(ctrlRemapper);
                ctrlRemapper.DisconnectEmulatedControllers();
                ctrlRemapper.Controller.Dispose();
            }
        }
        public List<IDSInputController> GetControllers() => controlRemapperList.Select((c) => c.Controller).ToList();
        public LuaRemapper GetControlRemapper(string controllerId) => controlRemapperList[GetControlRemapperIndex(controllerId)];
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
                if (control.RemapCompleted)
                {
                    control.RemapController();
                }
            }

        }
    }
}
