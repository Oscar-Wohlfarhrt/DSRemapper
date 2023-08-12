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
    /// <summary>
    /// Class used for loading all DSRemapper assemblies and plugins
    /// </summary>
    public class PluginsLoader
    {
        private static readonly List<Assembly> pluginAssemblies = new();
        /// <summary>
        /// Output plugins list, sorted by Emulated controller path
        /// </summary>
        public static SortedList<string, ConstructorInfo> OutputPlugins = new();
        /// <summary>
        /// Remapper plugins list, sorted by remapper asigned file extension
        /// </summary>
        public static SortedList<string, ConstructorInfo> RemapperPlugins = new();
        /// <summary>
        /// Physical devices scanner plugins (input plugins), sorted by namespace and class name
        /// </summary>
        public static SortedList<string, IDSDeviceScanner> Scanners = new();

        /// <summary>
        /// Loads the assemblies inside the DSRemapper Plugins folder and subfolders
        /// </summary>
        public static void LoadPluginAssemblies()
        {
            string[] plugins = Directory.GetFiles(DSPaths.PluginsPath, "*.dll", SearchOption.AllDirectories);
            foreach (string plugin in plugins)
            {
                try
                {
                    pluginAssemblies.Add(Assembly.LoadFrom(plugin));
                    Logger.Log($"Assembly found: {Path.GetRelativePath(DSPaths.PluginsPath, plugin)}");
                }
                catch
                {
                    Logger.LogWarning($"Unable to load {Path.GetRelativePath(DSPaths.PluginsPath, plugin)}");
                }
            }
        }

        /// <summary>
        /// Find and update all static lists of this class using the assemblies loaded by 'LoadPluginAssemblies' function
        /// </summary>
        public static void LoadPlugins()
        {
            IEnumerable<Type> types = pluginAssemblies.SelectMany(a => a.GetTypes());
            foreach (Type type in types)
            {
                if (type.IsInterface || !type.IsVisible)
                    continue;

                if (type.IsAssignableTo(typeof(IDSOutputController)))
                {
                    string? path = type.GetCustomAttribute<EmulatedControllerAttribute>()?.DevicePath;
                    if (path != null)
                    {
                        ConstructorInfo? ctr = type.GetConstructor(Type.EmptyTypes);
                        if (ctr != null)
                        {
                            if (OutputPlugins.TryAdd(path, ctr))
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
                        ConstructorInfo? ctr = type.GetConstructor(Type.EmptyTypes);
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
                    ConstructorInfo? ctr = type.GetConstructor(Type.EmptyTypes);
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
