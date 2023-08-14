using DSRemapper.ConfigManager;
using DSRemapper.Core;
using DSRemapper.DSLogger;
using System.Reflection;

namespace DSRemapper.DSInput
{
    public class DSInput
    {
        public DSInput() { }
        public static IDSInputDeviceInfo[] GetDevicesInfo()
        {
            return PluginsLoader.Scanners.SelectMany((s) => {
                return s.Value.ScanDevices();
            }).ToArray();
        }
    }
}