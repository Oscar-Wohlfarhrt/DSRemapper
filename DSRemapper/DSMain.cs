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

        public DSMain(string mainPagePath)
        {
            this.mainPagePath = mainPagePath;
            InitializeComponent();

            webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
            webView.EnsureCoreWebView2Async();
            this.mainPagePath = mainPagePath;
        }

        private void WebView_CoreWebView2InitializationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            webView.CoreWebView2.Navigate($"file:///{mainPagePath}");
            webView.CoreWebView2.AddHostObjectToScript("DSRBridge",new DSRBridge());
        }
    }
}
