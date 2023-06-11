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
#pragma warning disable CA1822 // Mark members as static because js can't access static methods
    public class DSRBridge
    {
        WebView2 webView;
        public DSRBridge(WebView2 webView)
        {
            this.webView = webView;
        }

        public void Log(string str)=> Logger.Log(str);
        public void LogWarning(string str) => Logger.LogWarning(str);
        public void LogError(string str) => Logger.LogError(str);

        public void LogConsole()
        {
            new DSMain("LogConsole.html").Show();
        }
        public void RegisterLogEvent(bool load=true)
        {
            Logger.OnLog -= OnLog_ToJS;

            if (load)
            {
                foreach (var log in Logger.logs)
                    OnLog_ToJS(log);
            }

            Logger.OnLog += OnLog_ToJS;
        }
        public void UnregisterLogEvent()
        {
            Logger.OnLog -= OnLog_ToJS;
        }
        public void OnLog_ToJS(Logger.LogEntry entry)
        {
            webView.Invoke(() =>
            {
                webView.CoreWebView2.ExecuteScriptAsync(@$"LogEvent(""{entry.ToString().Replace("\\","\\\\")}"")");
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
#pragma warning restore CA1822 // Mark members as static
}
