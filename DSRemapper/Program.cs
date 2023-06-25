using DSRemapper.DSLogger;
using DSRemapper.RazorLayouts;

namespace DSRemapper
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        { 
            ConfigManager.PluginsLoader.LoadPluginAssemblies();
            ConfigManager.PluginsLoader.LoadPlugins();
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            DSMain main = new DSMain();
            main.FormClosing += Main_FormClosing;
            Application.Run(main);
            RemapperCore.RemapperCore.Stop();
        }

        private static void Main_FormClosing(object? sender, FormClosingEventArgs e)
        {
            RemapperCore.RemapperCore.Stop();
        }
    }
}