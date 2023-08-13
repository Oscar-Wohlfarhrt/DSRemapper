using System.Reflection;

namespace DSRemapper.Core
{
    /// <summary>
    /// A class containing all the important folder paths for DSRemapper
    /// </summary>
    public static class DSPaths
    {
        private readonly static string ExePath = Assembly.GetExecutingAssembly().Location;
        /// <summary>
        /// Executing folder of the DSRemapper app
        /// </summary>
        public readonly static string ProgramPath = Path.GetDirectoryName(ExePath) ?? "";
        /// <summary>
        /// Folder containing all the DSRemapper plugins
        /// </summary>
        public readonly static string PluginsPath = Path.Combine(ProgramPath, "Plugins");
        /// <summary>
        /// Folder containing DSRemapper input plugins (exists for ordering purposes, there may be other plugins types)
        /// </summary>
        public readonly static string InputPluginsPath = Path.Combine(PluginsPath, "Input");
        /// <summary>
        /// Folder containing DSRemapper output plugins (exists for ordering purposes, there may be other plugins types)
        /// </summary>
        public readonly static string OutputPluginsPath = Path.Combine(PluginsPath, "Output");
        /// <summary>
        /// Folder containing DSRemapper remapper plugins (exists for ordering purposes, there may be other plugins types)
        /// </summary>
        public readonly static string RemapperPluginsPath = Path.Combine(PluginsPath, "Remapper");
        /// <summary>
        /// DSRemapper folder located inside users document folder (contains app and plugins configurations and remap profiles)
        /// </summary>
        public readonly static string FolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DSRemapper");
        /// <summary>
        /// Folder containing all remap profiles recogniced by the app.
        /// </summary>
        public readonly static string ProfilesPath = Path.Combine(FolderPath, "Profiles");
        /// <summary>
        /// Folder containing remapper profiles settings saved from them.
        /// </summary>
        public readonly static string ConfigPath = Path.Combine(FolderPath, "Configs");

        /// <summary>
        /// Static constructor that creates all folders if they doesn't exist, to prevent errors.
        /// </summary>
        static DSPaths()
        {
            string[] paths = {
                PluginsPath,
                InputPluginsPath,
                OutputPluginsPath,
                RemapperPluginsPath,
                FolderPath,
                ProfilesPath,
                ConfigPath
            };

            foreach (string path in paths)
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
        }
    }
}