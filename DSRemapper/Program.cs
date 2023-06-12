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
            Application.Run(new DSMain());
            Logger.Log("Program Out");
            RemapperCore.RemapperCore.StopScanner();
        }
    }
}