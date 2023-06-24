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
        private static List<Assembly> pluginAssemblies = new();
        public static SortedList<string, Type> InputPlugins = new();
        public static SortedList<string, ConstructorInfo> OutputPlugins = new();
        public static SortedList<string, ConstructorInfo> RemapperPlugins = new();
        //public static SortedList<string, Type> ScannerPlugins = new();
        public static SortedList<string, IDSDeviceScanner> Scanners = new();

        public static void LoadPluginAssemblies()
        {
            string[] plugins = Directory.GetFiles(DSPaths.PluginsPath, "*.dll", SearchOption.AllDirectories);
            foreach (string plugin in plugins)
            {
                pluginAssemblies.Add(Assembly.LoadFrom(plugin));
                Logger.Log($"Assembly found: {plugin}");
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
                        Logger.LogWarning($"Input plugin is duplicated: {type.FullName}");
                }
                else if (type.IsAssignableTo(typeof(IDSOutputController)))
                {
                    string? path = type.GetCustomAttribute<EmulatedControllerAttribute>()?.DevicePath;
                    if (path != null)
                    {
                        ConstructorInfo? ctr = type.GetConstructor(BindingFlags.Public, Type.EmptyTypes);
                        if (ctr != null)
                        {
                            if (OutputPlugins.TryAdd(path ?? type.FullName ?? "Unknown", ctr))
                                Logger.Log($"Output plugin found: {type.FullName}");
                            else
                                Logger.LogWarning($"Output plugin is duplicated: {type.FullName}");
                        }
                        else
                            Logger.LogWarning($"{type.FullName}: Output plugin doesn't have a public parameterless constructor");
                    }
                    else
                        Logger.LogWarning($"{type.FullName}: Output plugin doesn't have a path assigned");
                }
                else if (type.IsAssignableTo(typeof(IDSRemapper)))
                {
                    string? fileExt = type.GetCustomAttribute<RemapperAttribute>()?.FileExt;
                    if (fileExt != null)
                    {
                        ConstructorInfo? ctr = type.GetConstructor(BindingFlags.Public, Type.EmptyTypes);
                        if (ctr != null) {
                            if (RemapperPlugins.TryAdd(fileExt, ctr))
                            {
                                Logger.Log($"Remapper plugin found: {type.FullName}");
                            }
                            else
                                Logger.LogError($"{type.FullName}: Remapper plugin for extension \"{fileExt}\" is already loaded");
                        }
                        else
                            Logger.LogWarning($"{type.FullName}: Remapper plugin doesn't have a public parameterless constructor");
                    }
                    else
                        Logger.LogWarning($"{type.FullName}: Remapper plugin doesn't have a file extension assigned");
                }
                else if (type.IsAssignableTo(typeof(IDSDeviceScanner)))
                {
                    ConstructorInfo? ctr = type.GetConstructor(BindingFlags.Public, Type.EmptyTypes);
                    if (ctr != null)
                    {
                        if (Scanners.TryAdd(type.FullName ?? "Unknown", (IDSDeviceScanner)ctr.Invoke(null)))
                            Logger.Log($"Scanner plugin found: {type.FullName}");
                        else
                            Logger.LogWarning($"Scanner plugin is duplicated: {type.FullName}");
                    }
                    else
                        Logger.LogWarning($"{type.FullName}: Scanner plugin doesn't have a public parameterless constructor");
                }
            }
        }
    }
}
