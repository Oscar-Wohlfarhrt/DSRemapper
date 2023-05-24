using DSRemapper.Core;
using DSRemapper.DSLogger;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DSRemapper
{
    public class DSRBridge
    {
        WebView2 webView;
        public DSRBridge(WebView2 webView)
        {
            this.webView = webView;
        }
        public string test { get => "Hello JS!"; }
        public void LogOnConsole(string str)
        {
             Logger.Log(str);
        }
        public string LoopBack(string str)
        {
            return str;
        }
        public void LogConsole()
        {
            new DSMain("LogConsole.html").Show();
        }
        public void RegisterLogEvent()
        {
            Logger.OnLog += OnLog_ToJS;
        }
        public void OnLog_ToJS(Logger.LogEntry entry)
        {
            webView.Invoke(() =>
            {
                webView.CoreWebView2.ExecuteScriptAsync(@$"LogEvent(""{entry}"")");
            });
        }

        public void WindowsControllers()
        {
            new Process()
            {
                StartInfo = {
                            FileName = "cmd",
                            Arguments = "/C %SystemRoot%\\System32\\joy.cpl",
                            CreateNoWindow = true
                        }
            }.Start();
        }

        public IDSInputDeviceInfo[] GetDevicesInfo() => DSInput.DSInput.GetDevicesInfo();
    }
}
