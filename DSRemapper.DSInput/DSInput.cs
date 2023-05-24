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
                foreach (var dev in scanner.Value.ScanDevices())
                {
                    Logger.Log(dev.ToString());
                }
            }
        }

        public static IDSInputDeviceInfo[] GetDevicesInfo()
        {
            IDSInputDeviceInfo[] output= PluginsLoader.Scanners.SelectMany((s) => { return s.Value.ScanDevices(); } ).ToArray();
            return output;
        }
    }
}