using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;
using DSRemapper.DSInput;
using DSRemapper.Configs;
using DSRemapper.Remapper;
using System.Diagnostics;
using System.ComponentModel;

namespace DSRemapper
{
    public partial class MainForm : Form
    {
        readonly WebView2 webview = new ();
        readonly FileSystemWatcher fileUpdater = new();
        readonly RemapperCore remapper = new ();
        readonly BackgroundWorker remapWorker = new ();
        readonly BackgroundWorker scanWorker = new();
#if DEBUG
        public static readonly string webPath = "G:\\Projectos Visual\\DSRemapper\\DSRemapper\\Forms";
#else
        public static readonly string webPath = Path.GetFullPath("Forms");
#endif
        readonly string webPage = Path.Combine(webPath, "MainPage.html");

        public MainForm()
        {
            InitializeComponent();
            Controls.Add(webview);
            webview.Dock = DockStyle.Fill;

            fileUpdater.Path = webPath;
            fileUpdater.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            fileUpdater.Changed += FileUpdater_Changed;
            fileUpdater.Created += FileUpdater_Changed;
            fileUpdater.Deleted += FileUpdater_Changed;
            fileUpdater.IncludeSubdirectories = true;

            webview.CoreWebView2InitializationCompleted += Webview_CoreWebView2InitializationCompleted;
            webview.EnsureCoreWebView2Async();

            remapWorker.DoWork += RemapWorker_DoWork;
            scanWorker.DoWork += ScanWorker_DoWork;
        }

        private void Webview_CoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            webview.CoreWebView2.Navigate($"file:///{webPage}");
            webview.WebMessageReceived += Webview_WebMessageReceived;

            remapper.OnError += LuaRemapper_OnError;
            remapper.OnLog += LuaRemapper_OnLog;
            remapper.LoadDefaultProfile += LuaRemapper_LoadDefaultProfile;
            remapper.OnReportUpdate += LuaRemapper_OnReportUpdate;
            ControllerScanTimer.Enabled = true;
            ControllerUpdateTimer.Enabled = true;
            fileUpdater.EnableRaisingEvents = true;
        }

        private void LuaRemapper_OnError(object? sender, RemapperScriptEventArgs e)
        {
            Invoke(() =>
            {
                Console.WriteLine($"Error: Id: {e.id} | {e.message}");
                webview.CoreWebView2.ExecuteScriptAsync($@"LogOnConsole({JsonSerializer.Serialize(e)})");
            });
        }
        private void LuaRemapper_OnLog(object? sender, RemapperScriptEventArgs e)
        {
            Invoke(() =>
            {
                webview.CoreWebView2.ExecuteScriptAsync($@"LogOnConsole({JsonSerializer.Serialize(e)})");
            });
        }
        private void LuaRemapper_LoadDefaultProfile(object? sender, RemapperScriptEventArgs e)
        {
            //Console.WriteLine($"Load Profile: Id: {e.id} | {e.message}");
            Invoke(() =>
            {
                webview.CoreWebView2.ExecuteScriptAsync($@"SetProfile({JsonSerializer.Serialize(e)})");
            });
        }
        private void LuaRemapper_OnReportUpdate(object? sender, RemapperReportEventArgs e)
        {
            Invoke(()=>
            {
                string data = JsonSerializer.Serialize(e);
                webview.CoreWebView2.ExecuteScriptAsync($@"SetBattery({data})");
            });
        }

        private void FileUpdater_Changed(object sender, FileSystemEventArgs e)
        {
            Invoke(new Action(() => ReloadPage()));
        }
        private void ReloadPage()
        {
            webview.CoreWebView2.Reload();
        }
        struct WebMessageFormat
        {
            public string ControllerId { get; set; }
            public string ProfileName { get; set; }
            public int Command { get; set; }
        }
        private void Webview_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            WebMessageFormat mes = JsonSerializer.Deserialize<WebMessageFormat>(e.WebMessageAsJson);
            Console.WriteLine($"{mes.ControllerId} - {mes.ProfileName} - {mes.Command}");
            switch (mes.Command)
            {
                case -2:
                    new Process()
                    {
                        StartInfo = {
                            FileName = "cmd",
                            Arguments = "/C %SystemRoot%\\System32\\joy.cpl",
                            CreateNoWindow = true
                        }
                    }.Start();
                    break;
                case -1:
                    webview.CoreWebView2.ExecuteScriptAsync($"UpdateProfiles({JsonSerializer.Serialize(remapper.controlRemapperList.Select((r) => { return new WebMessageFormat { ControllerId = r.ControllerId, ProfileName = r.LastProfile }; } ))})");
                    break;
                case 0:
                    if(mes.ProfileName != string.Empty)
                        Console.WriteLine($"Set Profile to {mes.ProfileName} for {mes.ControllerId}");
                    else
                        Console.WriteLine($"Set Profile to None for {mes.ControllerId}");

                    remapper.SetControllerProfile(mes.ControllerId.Split("=")[1],mes.ProfileName);
                    break;
                case 1:
                    Console.WriteLine($"Test Input for {mes.ControllerId}");
                    new InputTest(remapper.GetControlRemapper(mes.ControllerId)).Show();
                    break;
                case 2:
                    Console.WriteLine($"Reload Profile for {mes.ControllerId}");
                    remapper.GetControlRemapper(mes.ControllerId).ReloadScript();
                    break;
                case 3:
                    Console.WriteLine($"Force disconnect for {mes.ControllerId}");
                    remapper.GetControlRemapper(mes.ControllerId).ForceDisconnect();
                    break;
            }
        }

        private void ControllerScanTimer_Tick(object sender, EventArgs e)
        {
            if (!scanWorker.IsBusy)
            {
                scanWorker.RunWorkerAsync();
            }
        }
        private void ScanWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            if (DSInputControllers.RefreshDevices())
            {
                Invoke(() =>
                {
                    ControllerUpdateTimer.Enabled = false;
                });

                while (remapWorker.IsBusy) ;

                List<IDSInputController> controllers = DSInputControllers.GetDevices().ToList();
                Invoke(() =>
                {
                    webview.CoreWebView2.ExecuteScriptAsync($"UpdateDeviceList({JsonSerializer.Serialize(controllers)})");
                    webview.CoreWebView2.ExecuteScriptAsync($"LoadProfiles({JsonSerializer.Serialize(ProfileManager.GetProfileNames())})");
                });
                remapper.SetControllers(controllers);

                Invoke(() =>
                {
                    ControllerUpdateTimer.Enabled = true;
                });
            }
        }

        private void ControllerUpdateTimer_Tick(object sender, EventArgs e)
        {
            remapper.RemapAll();
            /*if (!remapWorker.IsBusy)
            {
                remapWorker.RunWorkerAsync();
            }*/
        }

        private void RemapWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            remapper.RemapAll();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            ControllerScanTimer.Enabled = false;
            ControllerUpdateTimer.Enabled = false;

            foreach (var ctrl in remapper.controlRemapperList.ToArray())
            {
                remapper.RemoveController(ctrl.ControllerId);
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }
    }

    [ComVisible(true)]
    public class WebBridge
    {

    }
}