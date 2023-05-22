﻿using DSRemapper.Core;
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
        public static SortedList<string, Type> InputPlugins = new();
        public static SortedList<string, Type> OutputPlugins = new();
        public static SortedList<string, Type> RemapperPlugins = new();
        public static SortedList<string, Type> ScannerPlugins = new();
        public static SortedList<string, IDSDeviceScanner<IDSInputController>> Scanners = new();

        public static void LoadPluginAssemblies()
        {
            string[] plugins = Directory.GetFiles(DSPaths.PluginsPath, "*.dll", SearchOption.AllDirectories);
            foreach (string plugin in plugins)
            {
                Logger.Log($"Assembly found: {plugin}");
                Assembly.LoadFile(plugin);
            }
        }

        public static void LoadPlugins()
        {
            IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes());
            foreach (Type type in types)
            {
                if (type.IsInterface)
                    continue;

                if (type.IsAssignableTo(typeof(IDSInputController)))
                {
                    if (!InputPlugins.TryAdd(type.FullName ?? "Unknown", type))
                        Logger.Log($"Input plugin found: {type.FullName}");
                    else
                        Logger.LogError($"Input plugin duplicated: {type.FullName}");
                }
                else if (type.IsAssignableTo(typeof(IDSOutputController)))
                {
                    if (!OutputPlugins.TryAdd(type.FullName ?? "Unknown", type))
                        Logger.Log($"Output plugin found: {type.FullName}");
                    else
                        Logger.LogError($"Output plugin duplicated: {type.FullName}");
                }
                else if (type.IsAssignableTo(typeof(IDSRemapper)))
                {
                    if (!RemapperPlugins.TryAdd(type.FullName ?? "Unknown", type))
                        Logger.Log($"Remapper plugin found: {type.FullName}");
                    else
                        Logger.LogError($"Remapper plugin duplicated: {type.FullName}");
                }
                else if (type.IsAssignableTo(typeof(IDSDeviceScanner<>)))
                {
                    if (!ScannerPlugins.TryAdd(type.FullName ?? "Unknown", type))
                    {
                        Logger.Log($"Scanner plugin found: {type.FullName}");
                    }
                    else
                        Logger.LogError($"Scanner plugin duplicated: {type.FullName}");
                }
            }
        }
    }
}