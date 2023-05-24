using DSRemapper.ConfigManager;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DSRemapper
{
    public partial class DSMain : Form
    {
        string mainPagePath;
        FileSystemWatcher fileWatcher;

#if DEBUG
        public static readonly string formsPath = "G:\\Projectos Visual\\DSRemapper\\DSRemapper\\Forms";
#else
        public static readonly string formsPath = DSPaths.FormsPath;
#endif
        //G:\Projectos Visual\DSRemapper\DSRemapper\Forms
        public DSMain(string mainPagePath)
        {
            this.mainPagePath = Path.Combine(formsPath, mainPagePath);
            InitializeComponent();

            fileWatcher = new(formsPath)
            {
                EnableRaisingEvents = false
            };
            fileWatcher.Created += FileWatcher_Update;
            fileWatcher.Deleted += FileWatcher_Update;
            fileWatcher.Changed += FileWatcher_Update;
            fileWatcher.Renamed += FileWatcher_Update;

            webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
            webView.EnsureCoreWebView2Async();
        }

        private void FileWatcher_Update(object sender, FileSystemEventArgs e)
        {
            Invoke(() =>
            {
                webView.CoreWebView2.Navigate($"file:///{mainPagePath}");
            });
        }

        private void WebView_CoreWebView2InitializationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            webView.CoreWebView2.Navigate($"file:///{mainPagePath}");

            webView.CoreWebView2.Settings.AreHostObjectsAllowed = true;
            webView.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = true;
            webView.CoreWebView2.Settings.IsScriptEnabled = true;
            webView.CoreWebView2.Settings.IsWebMessageEnabled = true;

            webView.CoreWebView2.AddHostObjectToScript("DSRBridge",new DSRBridge(webView));
            fileWatcher.EnableRaisingEvents = true;
        }
    }
}
