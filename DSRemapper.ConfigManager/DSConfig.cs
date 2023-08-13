using DSRemapper.Core;
using System.Text.Json;

namespace DSRemapper.ConfigManager
{
    /// <summary>
    /// Physical controller DSRemapper config class. Manage all configs associated with physical devices inside DSRemapper.
    /// </summary>
    [Serializable]
    public class RemapperConfig
    {
        /// <summary>
        /// Physical controller unique id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Auto connect field value. If is true, physical controller is automatically connected, when plugged, for remapping.
        /// </summary>
        public bool AutoConnect { get; set; }
        /// <summary>
        /// Las remap profile associated with the physical controller
        /// </summary>
        public string LastProfile { get; set; }
        /// <summary>
        /// RemapperConfig class constructor
        /// </summary>
        /// <param name="id">Physical controller id for the config object</param>
        public RemapperConfig(string id)
        {
            Id = id;
            AutoConnect = false;
            LastProfile = "";
        }
    }
    /// <summary>
    /// Manage saveand load of the physical controllers configs
    /// </summary>
    public static class DSConfig
    {
        private static string configPath = Path.Combine(DSPaths.ConfigPath, "DSConfigs.json");
        private static List<RemapperConfig> remapperConfigs = null!;

        /// <summary>
        /// DSConfig static contructor
        /// </summary>
        static DSConfig()
        {
            if (File.Exists(configPath))
                LoadConfigFile();
            else
                remapperConfigs = new();
        }
        private static void LoadConfigFile() => remapperConfigs = JsonSerializer
            .Deserialize<List<RemapperConfig>>(File.ReadAllText(configPath)) ?? new();
        private static void SaveConfigFile() =>
            File.WriteAllText(configPath, JsonSerializer.Serialize(remapperConfigs));
        /// <summary>
        /// Gets the controller config associated with the id.
        /// </summary>
        /// <param name="id">Id of a physical controller</param>
        /// <returns></returns>
        public static RemapperConfig GetConfig(string id)
        {
            if (!remapperConfigs.Exists((c) => c.Id == id))
                remapperConfigs.Add(new RemapperConfig(id));

#pragma warning disable CS8603 // Posible tipo de valor devuelto de referencia nulo
            return remapperConfigs.Find((c) => c.Id == id);
#pragma warning restore CS8603 // Posible tipo de valor devuelto de referencia nulo
        }

        /// <summary>
        /// Sets the last profile field for the physical controller associated with the id.
        /// </summary>
        /// <param name="id">Id of a physical controller</param>
        /// <param name="profile">Profile path (relative to DSRemapper Profiles folder) of the last assigned profile</param>
        public static void SetLastProfile(string id, string profile)
        {
            GetConfig(id).LastProfile = profile;
            SaveConfigFile();
        }
        /// <summary>
        /// Sets the auto connect field for the physical controller associated with the id.
        /// </summary>
        /// <param name="id">Id of a physical controller</param>
        /// <param name="autoConnect">True if the controller has to be connected as soon as it is plugged in</param>
        public static void SetAutoConnect(string id, bool autoConnect)
        {
            GetConfig(id).AutoConnect = autoConnect;
            SaveConfigFile();
        }
    }
}
