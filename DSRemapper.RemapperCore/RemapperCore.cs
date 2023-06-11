using DSRemapper.Core;
using System.Threading;

namespace DSRemapper.RemapperCore
{
    public static class RemapperCore
    {
        public static List<Remapper> remappers = new List<Remapper>();

        public static void SetControllers(List<IDSInputController> controllers)
        {
            List<Remapper> removeList = new List<Remapper>();
            foreach (var ctrl in remappers)
            {
                if (!controllers.Exists((c) => { return c.Id == ctrl.Id; }))
                    removeList.Add(ctrl);
            }

            foreach (var ctrl in removeList)
                RemoveController(ctrl.Id);

            foreach (var ctrl in controllers)
                AddController(ctrl);
        }

        public static void AddController(IDSInputController controller, string? profileName = null)
        {
            if (!remappers.Exists((c) => { return c.Id == controller.Id; }))
            {
                Remapper ctrlRemapper;

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
        public static void RemoveController(string controllerId)
        {
            Remapper? ctrlRemapper = controlRemapperList.Find((c) => { return c.ControllerId == controllerId; });

            if (ctrlRemapper != null)
            {
                remappers.Remove(ctrlRemapper);
                ctrlRemapper.DisconnectEmulatedControllers();
                ctrlRemapper.Controller.Dispose();
            }
        }
    }

    public class Remapper : IDisposable
    {
        private readonly IDSInputController controller;
        private readonly IDSRemapper remapper;
        private readonly Thread thread;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly CancellationToken cancellationToken;
        //Timer timer;

        public Remapper(IDSInputController controller, IDSRemapper remapper)
        {
            this.controller = controller;
            this.remapper = remapper;
            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;

            /*timer = new Timer(RemapTimer);
            timer.Change(0,10);*/

            thread = new(RemapThread)
            {
                Name = $"{controller.Name} Remapper",
                Priority = ThreadPriority.Normal
            };

            /*Task.Factory.StartNew(RemapThread, CancellationToken.None,
                TaskCreationOptions.LongRunning, TaskScheduler.Default);*/
        }

        public string Id => controller.Id;
        public string Name => controller.Name;
        public string Type => controller.Type;
        public bool IsConnected => controller.IsConnected;
        public void Connect()
        {
            if (!IsConnected)
                controller.Connect();
        }
        public void Disconnect()
        {
            if (IsConnected)
                controller.Disconnect();
        }

        public void Dispose()
        {
            Stop();
        }

        public void Start()
        {
            thread.Start();
        }

        public void Stop()
        {
            cancellationTokenSource.Cancel();
        }

        private void RemapThread()
        {
            while (cancellationToken.IsCancellationRequested)
            {
                if(IsConnected)
                    controller.SendOutputReport(remapper.Remap(controller.GetInputReport()));

                Thread.Sleep(10);
            }
        }
        private void RemapTimer(object? sender)
        {

        }
    }
}