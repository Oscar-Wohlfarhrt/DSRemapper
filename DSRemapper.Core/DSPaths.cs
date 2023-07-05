using System.Reflection;

namespace DSRemapper.Core
{
    public static class DSPaths
    {
        private readonly static string ExePath = Assembly.GetExecutingAssembly().Location;
        public readonly static string ProgramPath = Path.GetDirectoryName(ExePath) ?? "";
        public readonly static string FormsPath = Path.Combine(ProgramPath, "Forms");
        public readonly static string PluginsPath = Path.Combine(ProgramPath, "Plugins");
        public readonly static string InputPluginsPath = Path.Combine(PluginsPath, "Input");
        public readonly static string OutputPluginsPath = Path.Combine(PluginsPath, "Output");
        public readonly static string RemapperPluginsPath = Path.Combine(PluginsPath, "Remapper");
        public readonly static string FolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DSRemapper");
        public readonly static string ProfilesPath = Path.Combine(FolderPath, "Profiles");
        public readonly static string ConfigPath = Path.Combine(FolderPath, "Configs");

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