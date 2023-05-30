using DSRemapper.Core;
using DSRemapper.DSLogger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DSRemapper.ConfigManager
{
    
    public class PluginsLoader
    {
        private class DefaultScanner : IDSDeviceScanner
        {
            public IDSInputDeviceInfo[] ScanDevices() => Array.Empty<IDSInputDeviceInfo>();
        }

        private static List<Assembly> pluginAssemblies = new();
        public static SortedList<string, Type> InputPlugins = new();
        public static SortedList<string, Type> OutputPlugins = new();
        public static SortedList<string, Type> RemapperPlugins = new();
        public static SortedList<string, Type> ScannerPlugins = new();
        public static SortedList<string, IDSDeviceScanner> Scanners = new();

        public static void LoadPluginAssemblies()
        {
            string[] plugins = Directory.GetFiles(DSPaths.PluginsPath, "*.dll", SearchOption.AllDirectories);
            foreach (string plugin in plugins)
            {
                Logger.Log($"Assembly found: {plugin}");
                pluginAssemblies.Add(Assembly.LoadFrom(plugin));
            }
        }

        public static void LoadPlugins()
        {
            IEnumerable<Type> types = pluginAssemblies.SelectMany(a => a.GetTypes());
            foreach (Type type in types)
            {
                if (type.IsInterface || !type.IsVisible)
                    continue;

                if (type.IsAssignableTo(typeof(IDSInputController)))
                {
                    if (InputPlugins.TryAdd(type.FullName ?? "Unknown", type))
                        Logger.Log($"Input plugin found: {type.FullName}");
                    else
                        Logger.LogError($"Input plugin duplicated: {type.FullName}");
                }
                else if (type.IsAssignableTo(typeof(IDSOutputController)))
                {
                    if (OutputPlugins.TryAdd(type.FullName ?? "Unknown", type))
                        Logger.Log($"Output plugin found: {type.FullName}");
                    else
                        Logger.LogError($"Output plugin duplicated: {type.FullName}");
                }
                else if (type.IsAssignableTo(typeof(IDSRemapper)))
                {
                    if (RemapperPlugins.TryAdd(type.FullName ?? "Unknown", type))
                        Logger.Log($"Remapper plugin found: {type.FullName}");
                    else
                        Logger.LogError($"Remapper plugin duplicated: {type.FullName}");
                }
                else if (type.IsAssignableTo(typeof(IDSDeviceScanner)))
                {
                    if (Scanners.TryAdd(type.FullName ?? "Unknown", (IDSDeviceScanner)(Activator.CreateInstance(type)??new DefaultScanner())))
                        Logger.Log($"Scanner plugin found: {type.FullName}");
                    else
                        Logger.LogError($"Scanner plugin duplicated: {type.FullName}");
                }
            }
        }
    }
}
