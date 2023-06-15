using DSRemapper.Core;
using DSRemapper.DSInput;
using DSRemapper.Types;
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
                tokenSource = new();
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
                if(devs.Length != remappers.Count)
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
        private CancellationTokenSource cancellationTokenSource;
        private CancellationToken cancellationToken;
        //Timer timer;

        public delegate void ControllerRead(DSInputReport report);
        public event ControllerRead? OnRead;
        public int OnReadSubscriptors => OnRead?.GetInvocationList().Length ?? 0;

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
        public bool IsRunning => thread?.IsAlive ?? false;
        public bool Connect()
        {
            if (!IsConnected)
                controller.Connect();

            return IsConnected;
        }
        public bool Disconnect()
        {
            if (IsConnected)
                controller.Disconnect();

            return IsConnected;
        }

        public void Dispose()
        {
            Stop();
            controller.Dispose();
        }

        public void Start()
        {
            Stop();
            if (Connect())
            {
                cancellationTokenSource = new CancellationTokenSource();
                cancellationToken = cancellationTokenSource.Token;
                thread = new(RemapThread)
                {
                    Name = $"{controller.Name} Remapper",
                    Priority = ThreadPriority.Normal
                };
                thread.Start();
            }
        }

        public void Stop()
        {
            cancellationTokenSource.Cancel();
            if (thread!=null && thread.IsAlive)
            {
                thread.Join();
                Disconnect();
            }
        }

        private void RemapThread()
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (IsConnected)
                {
                    DSInputReport report = controller.GetInputReport();
                    OnRead?.Invoke(report);
                    if (remapper != null)
                    {
                        controller.SendOutputReport(remapper.Remap(report));
                    }
                }

                Thread.Sleep(10);
            }
        }
        /*private void RemapTimer(object? sender)
        {

        }*/
    }
}