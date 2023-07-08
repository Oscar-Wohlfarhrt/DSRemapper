using DSRemapper.RazorLayouts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSRemapper
{
    internal static class DSBridge
    {
        public static DSForm<ConsoleLayout>? console = null;
        /// <summary>
        /// 
        /// </summary>
        public static void WindowsControllers()
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
        public static void LogConsole()
        {
            if (console == null || console.IsDisposed)
            {
                console = new DSForm<ConsoleLayout>();
                console.Show();
            }
            else
            {
                if(console.WindowState == FormWindowState.Minimized)
                    console.WindowState = FormWindowState.Normal;
                console.Focus();
            }
        }
    }
}
