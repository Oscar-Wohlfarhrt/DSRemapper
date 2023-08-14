using DSRemapper.ConfigManager;
using DSRemapper.Core;
using DSRemapper.DSLogger;
using DSRemapper.Types;
using System.Reflection;

namespace DSRemapper.RemapperCore
{
    /// <summary>
    /// Main class of DSRemapper program. Controls when a supported device is plugged and unplugged, and all remappers in charge of remap the controllers
    /// </summary>
    public static class RemapperCore
    {
        /// <summary>
        /// Remappers list of DSRemapper, referencing all the plugged and detected devices.
        /// </summary>
        public static List<Remapper> Remappers { get; private set; } = new();

        private static Thread? deviceScannerThread;
        private static CancellationTokenSource tokenSource = new();
        private static CancellationToken cancellationToken;
        /// <summary>
        /// Delegate for RemapperCore device updates
        /// </summary>
        public delegate void RemapperUpdateArgs();
        /// <summary>
        /// Occurs when a new devices are detected by DSRemapper
        /// </summary>
        public static event RemapperUpdateArgs? OnUpdate;
        /// <summary>
        /// Starts the device scanner thread of DSRemapper
        /// </summary>
        public static void StartScanner()
        {
            StopScanner();
            tokenSource = new();
            cancellationToken = tokenSource.Token;
            deviceScannerThread = new(DeviceScanner)
            {
                Name = $"DSRemapper Device Scanner",
                Priority = ThreadPriority.BelowNormal
            };
            deviceScannerThread.Start();
        }
        /// <summary>
        /// Stops the device scanner thread of DSRemapper
        /// </summary>
        public static void StopScanner()
        {
            tokenSource.Cancel();
            deviceScannerThread?.Join();
        }
        /// <summary>
        /// Global devices information retrieval function
        /// </summary>
        /// <returns>An array with all devices plugged to the computer and recogniced by DSRemapper</returns>
        public static IDSInputDeviceInfo[] GetDevicesInfo() => PluginsLoader.Scanners
            .SelectMany((s) => s.Value.ScanDevices()).ToArray();
        private static void DeviceScanner()
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var devs = GetDevicesInfo();
                if(devs.Length != Remappers.Count)
                {
                    SetControllers(devs.Select((i) => i.CreateController()).ToList());
                    OnUpdate?.Invoke();
                }
                Thread.Sleep(1000);
            }
        }
        /// <summary>
        /// Stops all remapper threads and device scanner thread
        /// </summary>
        public static void Stop()
        {
            StopScanner();
            DisposeAllRemappers();
        }
        /// <summary>
        /// Dispose all remappers
        /// </summary>
        public static void DisposeAllRemappers()
        {
            foreach (Remapper r in Remappers)
                r.Dispose();
        }
        /// <summary>
        /// Set RemapperCore devices, adding the new ones and deleting the unplugged ones
        /// </summary>
        /// <param name="controllers">List of current controllers</param>
        public static void SetControllers(List<IDSInputController> controllers)
        {
            List<Remapper> removeList = new();
            foreach (var rmp in Remappers)
            {
                if (!controllers.Exists((c) => { return c.Id == rmp.Id; }))
                    removeList.Add(rmp);
            }

            foreach (var ctrl in removeList)
                RemoveController(ctrl.Id);

            foreach (var ctrl in controllers)
                AddController(ctrl);
        }
        /// <summary>
        /// Adds a new controller to RemapperCore class
        /// </summary>
        /// <param name="controller">New input controller</param>
        private static void AddController(IDSInputController controller)
        {
            if (!Remappers.Exists((c) => { return c.Id == controller.Id; }))
            {
                Logger.Log($"Physical device plugged: {controller.Name} [{controller.Id}]");
                Remappers.Add(new(controller));
            }
        }
        /// <summary>
        /// Adds a new controller to RemapperCore class
        /// </summary>
        /// <param name="controllerId">The id of the controller to disconnect</param>
        private static void RemoveController(string controllerId)
        {
            Remapper? ctrlRemapper = Remappers.Find((c) => { return c.Id == controllerId; });

            if (ctrlRemapper != null)
            {
                Logger.Log($"Physical device unpluged: {ctrlRemapper.Name} [{ctrlRemapper.Id}]");
                Remappers.Remove(ctrlRemapper);
                ctrlRemapper.Dispose();
            }
        }
        /// <summary>
        /// Creates a new remapper object using the remap profile file extension to get the right one.
        /// </summary>
        /// <param name="fileExt"></param>
        /// <returns></returns>
        internal static IDSRemapper? CreateRemapper(string fileExt)
        {
            if (PluginsLoader.RemapperPlugins.TryGetValue(fileExt,out ConstructorInfo? remapType))
                return (IDSRemapper?)remapType?.Invoke(null);

            return null;
        }
        /// <summary>
        /// Reload/update all remappers current profiles
        /// </summary>
        public static void ReloadAllProfiles()
        {
            foreach (var rmp in Remappers)
                rmp.ReloadProfile();
        }
    }
    /// <summary>
    /// Remapper container class. Contains the thread and the remapper plugin in charge of remapping the controller.
    /// </summary>
    public class Remapper : IDisposable
    {
        private readonly IDSInputController controller;
        private IDSRemapper? remapper = null;
        private Thread? thread=null;
        private CancellationTokenSource cancellationTokenSource;
        private CancellationToken cancellationToken;
        /// <summary>
        /// Delegate for the ControllerRead event
        /// </summary>
        /// <param name="report"></param>
        public delegate void ControllerRead(DSInputReport report);
        /// <summary>
        /// Occurs when a DSRemapper standard input report from the controller is readed
        /// </summary>
        public event ControllerRead? OnRead;
        /// <summary>
        /// Gets all the subscriptions to the 'OnRead' event. For debugging purposes.
        /// </summary>
        public int OnReadSubscriptors => OnRead?.GetInvocationList().Length ?? 0;
        /// <summary>
        /// Occurs when the remapper plugin invokes an OnLog event.
        /// </summary>
        public event RemapperEventArgs? OnLog;
        /// <summary>
        /// Remapper class constructor
        /// </summary>
        /// <param name="controller">The controller asigned to the remapper</param>
        public Remapper(IDSInputController controller)
        {
            this.controller = controller;
            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;
        }
        /// <inheritdoc cref="IDSInputController.Id"/>
        public string Id => controller.Id;
        /// <inheritdoc cref="IDSInputController.Name"/>
        public string Name => controller.Name;
        /// <inheritdoc cref="IDSInputController.Type"/>
        public string Type => controller.Type;
        /// <inheritdoc cref="IDSInputController.IsConnected"/>
        public bool IsConnected => controller.IsConnected;
        /// <summary>
        /// Gets if the remapper is runing it's thread.
        /// </summary>
        public bool IsRunning => thread?.IsAlive ?? false;
        /// <summary>
        /// Gets the current assigned remap profile of the remapper.
        /// </summary>
        public string CurrentProfile { get; private set; } = "";

        /// <inheritdoc cref="IDSInputController.Connect"/>
        public bool Connect()
        {
            if (!IsConnected)
                controller.Connect();

            return IsConnected;
        }
        /// <inheritdoc cref="IDSInputController.Disconnect"/>
        public bool Disconnect()
        {
            if (IsConnected)
                controller.Disconnect();

            return IsConnected;
        }
        /// <inheritdoc/>
        public void Dispose()
        {
            Stop();
            controller.Dispose();
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Starts the remapper thread for remapping the controller
        /// </summary>
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
        /// <summary>
        /// Stops the remapper thread for remapping the controller
        /// </summary>
        public void Stop()
        {
            cancellationTokenSource.Cancel();
            if (thread!=null && thread.IsAlive)
            {
                thread.Join();
                Disconnect();
            }
        }
        /// <summary>
        /// Sets the profile to the remapper plugin object used for the remap.
        /// </summary>
        /// <param name="profile">File path to the remap profile</param>
        public void SetProfile(string profile)
        {
            if (profile == "")
            {
                if (remapper != null)
                    remapper.OnLog -= OnLog;
                remapper?.Dispose();
                remapper = null;
            }
            else
            {
                string fullPath = Path.Combine(DSPaths.ProfilesPath, profile);
                if (File.Exists(fullPath))
                {
                    string ext = Path.GetExtension(fullPath)[1..];
                    remapper = RemapperCore.CreateRemapper(ext);
                    if (remapper!=null)
                    {
                        remapper.OnLog += OnLog;
                        remapper.SetScript(fullPath);
                    }
                }
            }
            CurrentProfile=profile;
        }
        /// <summary>
        /// Reloads the current profile on the remapper plugin object.
        /// If the remap profile file is changed, this function has to be called for the changes to take effect.
        /// </summary>
        public void ReloadProfile() => SetProfile(CurrentProfile);
        private void RemapThread()
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
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
                }
                catch (Exception e) {
                    Logger.LogError($"{e.Source}:\n{e.Message}");
                }

                Thread.Sleep(20);
            }
        }
    }
}