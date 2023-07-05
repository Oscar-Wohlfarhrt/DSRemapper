using DSRemapper.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DSRemapper.ConfigManager
{
    [Serializable]
    public class RemapperConfig
    {
        public string Id { get; set; }
        public bool AutoConnect { get; set; }
        public string LastProfile { get; set; }
        public RemapperConfig(string id)
        {
            Id = id;
            AutoConnect = false;
            LastProfile = "";
        }
    }

    public static class DSConfig
    {
        private static string configPath = Path.Combine(DSPaths.ConfigPath,"DSConfigs.json");
        private static List<RemapperConfig> remapperConfigs;

        static DSConfig()
        {
            if(File.Exists(configPath))
                LoadConfigFile();
            else
                remapperConfigs = new();
        }
        private static void LoadConfigFile() => remapperConfigs = JsonSerializer
            .Deserialize<List<RemapperConfig>>(File.ReadAllText(configPath)) ?? new();
        private static void SaveConfigFile() => File.
            WriteAllText(configPath, JsonSerializer.Serialize(remapperConfigs));
        public static RemapperConfig GetConfig(string id)
        {
            if(!remapperConfigs.Exists((c) => c.Id == id))
                remapperConfigs.Add(new RemapperConfig(id));

#pragma warning disable CS8603 // Posible tipo de valor devuelto de referencia nulo
            return remapperConfigs.Find((c) => c.Id == id);
#pragma warning restore CS8603 // Posible tipo de valor devuelto de referencia nulo
        }

        public static void SetLastProfile(string id, string profile)
        {
            GetConfig(id).LastProfile = profile;
            SaveConfigFile();
        }
        public static void SetAutoConnect(string id,bool autoConnect)
        {
            GetConfig(id).AutoConnect = autoConnect;
            SaveConfigFile();
        }
    }
}
