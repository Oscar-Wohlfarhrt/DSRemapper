using DSRemapper.ConfigManager;
using DSRemapper.Core;
using DSRemapper.DSLogger;
using System.Reflection;

namespace DSRemapper.DSInput
{
    public class DSInput
    {
        public DSInput() { }

        public static void LogDevices()
        {
            foreach(var scanner in PluginsLoader.Scanners)
            {
                var scan= (IDSDeviceScanner)Activator.CreateInstance(scanner.Value);
                if(scan.ScanDevices())
            }
        }
    }
}