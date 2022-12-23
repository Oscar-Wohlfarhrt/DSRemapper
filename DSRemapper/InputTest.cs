using DSRemapper;
using DSRemapper.Remapper;
using Microsoft.Web.WebView2.WinForms;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows.Forms;
using Windows.Devices.Geolocation;
using Windows.UI.Notifications;
using static DSRemapper.Remapper.LuaRemapper;

namespace DSRemapper
{
    public partial class InputTest : Form
    {
        readonly WebView2 webview = new();
        readonly FileSystemWatcher fileUpdater;
        readonly IRemapper remapper;
        readonly string webPage = Path.Combine(MainForm.webPath, "InputTest.html");

        public InputTest(IRemapper remapper)
        {
            InitializeComponent();
            Controls.Add(webview);
            webview.Dock = DockStyle.Fill;

            this.remapper = remapper;

            fileUpdater = new FileSystemWatcher();
            fileUpdater.Path = MainForm.webPath;
            fileUpdater.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            fileUpdater.Changed += FileUpdater_Changed;
            fileUpdater.Created += FileUpdater_Changed;
            fileUpdater.Deleted += FileUpdater_Changed;
            fileUpdater.IncludeSubdirectories = true;

            webview.CoreWebView2InitializationCompleted += Webview_CoreWebView2InitializationCompleted; ;
            webview.EnsureCoreWebView2Async();
        }

        private void Webview_CoreWebView2InitializationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            webview.CoreWebView2.Navigate($"file:///{webPage}");
            remapper.OnReportUpdate += Remapper_OnReportUpdate;
            fileUpdater.EnableRaisingEvents = true;
        }

        private void Remapper_OnReportUpdate(object? sender, RemapperReportEventArgs e)
        {
            Invoke(delegate
            {
                if (webview.CoreWebView2 != null)
                {
                    string data = JsonSerializer.Serialize(e.report);
                    webview.CoreWebView2.ExecuteScriptAsync($"SetData({JsonSerializer.Serialize(e.report)})");
                }
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

        private void InputTest_FormClosed(object sender, FormClosedEventArgs e)
        {
            fileUpdater.Dispose();
            Console.WriteLine("InputTest closed");
            remapper.OnReportUpdate -= Remapper_OnReportUpdate;
            Dispose();
        }
    }
}
