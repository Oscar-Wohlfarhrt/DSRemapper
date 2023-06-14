using DSRemapper.Core;
using DSRemapper.DSInput;
using System.Threading;
using System.Xml.Linq;

namespace DSRemapper.RemapperCore
{
    public static class RemapperCore
    {
        public static List<Remapper> remappers = new List<Remapper>();

        public static Thread? deviceScannerThread;
        private static CancellationTokenSource tokenSource = new CancellationTokenSource();
        private static CancellationToken cancellationToken;

        public delegate void RemapperUpdateArgs(List<Remapper> remappers);
        public static event RemapperUpdateArgs? OnUpdate;

        public static void StartScanner()
        {
            if (deviceScannerThread != null)
            {
                StopScanner();
                tokenSource.TryReset();
                cancellationToken = tokenSource.Token;
            }
            deviceScannerThread = new(DeviceScanner)
            {
                Name = $"DSRemapper Device Scanner",
                Priority = ThreadPriority.BelowNormal
            };
            deviceScannerThread.Start();
        }
        public static void StopScanner()
        {
            tokenSource.Cancel();
            deviceScannerThread?.Join();
        }
        private static void DeviceScanner()
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var devs = DSInput.DSInput.GetDevicesInfo();
                if(devs.Length!=remappers.Count)
                {
                    SetControllers(devs.Select((i) => i.CreateController()).ToList());
                    OnUpdate?.Invoke(remappers);
                }
                Thread.Sleep(1000);
            }
        }

        public static void SetControllers(List<IDSInputController> controllers)
        {
            List<Remapper> removeList = new List<Remapper>();
            foreach (var rmp in remappers)
            {
                if (!controllers.Exists((c) => { return c.Id == rmp.Id; }))
                    removeList.Add(rmp);
            }

            foreach (var ctrl in removeList)
                RemoveController(ctrl.Id);

            foreach (var ctrl in controllers)
                AddController(ctrl);
        }

        private static void AddController(IDSInputController controller)
        {
            if (!remappers.Exists((c) => { return c.Id == controller.Id; }))
            {
                remappers.Add(new(controller));
            }
        }
        private static void RemoveController(string controllerId)
        {
            Remapper? ctrlRemapper = remappers.Find((c) => { return c.Id == controllerId; });

            if (ctrlRemapper != null)
            {
                remappers.Remove(ctrlRemapper);
                ctrlRemapper.Dispose();
            }
        }
    }
    public enum RemapperEventType
    {
        DeviceConsole,
        Warning,
        Error
    }
    delegate void RemapperEventArgs(RemapperEventType type, string message);
    public class Remapper : IDisposable
    {
        private readonly IDSInputController controller;
        private IDSRemapper? remapper = null;
        private Thread? thread=null;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly CancellationToken cancellationToken;
        //Timer timer;

        event RemapperEventArgs? OnLog;

        public Remapper(IDSInputController controller)
        {
            this.controller = controller;
            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;

            /*timer = new Timer(RemapTimer);
            timer.Change(0,10);*/


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
            controller.Dispose();
        }

        public void Start()
        {
            Stop();
            thread = new(RemapThread)
            {
                Name = $"{controller.Name} Remapper",
                Priority = ThreadPriority.Normal
            };
            thread.Start();
        }

        public void Stop()
        {
            if (thread!=null && thread.IsAlive)
            {
                cancellationTokenSource.Cancel();
                thread.Join();
            }
        }

        private void RemapThread()
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if(IsConnected && remapper!=null)
                    controller.SendOutputReport(remapper.Remap(controller.GetInputReport()));

                Thread.Sleep(10);
            }
        }
        /*private void RemapTimer(object? sender)
        {

        }*/
    }
}