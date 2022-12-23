using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.System.Profile;

namespace DSRemapper.Configs
{
    internal class ProfileManager
    {
        internal struct LastProfile
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }
        public static string mainDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DSRemapper");
        public static string profilesDir = Path.Combine(mainDir, "Profiles");
        public static string lastProfilesPath = Path.Combine(mainDir, "lastProfiles.json");
        public static string profileExt = "lua";

        public static List<LastProfile>? lastProfiles = null;

        public ProfileManager()
        {
            foreach (string file in GetProfileNames())
            {
                Console.WriteLine(file);
            }
        }

        private static string[] GetProfilesDir(string dir)
        {
            return Directory.GetFiles(dir, $"*.*",SearchOption.AllDirectories);
        }
        public static string[] GetProfileNames()
        {
            return GetProfilesDir(profilesDir)
                .Select((p) => { return Path.GetRelativePath(profilesDir,p); })
                .ToArray();
        }
        public static string GetProfileByName(string name)
        {
            return File.ReadAllText(GetProfilePathByName(name));
        }
        public static string GetProfilePathByName(string name)
        {
            string? profile = GetProfilesDir(profilesDir).ToList()
                .Find((p) => { return Path.GetRelativePath(profilesDir, p) == name; });

            if (profile == null)
                throw new Exception("Profile not found");

            return profile;
        }

        private static void LoadLastProfiles()
        {
            if (lastProfiles == null)
            {
                if (File.Exists(lastProfilesPath))
                {
                    lastProfiles = JsonSerializer.Deserialize<List<LastProfile>>(File.ReadAllText(lastProfilesPath));
                }
                else
                {
                    lastProfiles = new List<LastProfile>();
                }
            }
        }
#pragma warning disable CS8602 // Desreferencia de una referencia posiblemente NULL.
        public static void SetLastProfile(string id, string name)
        {
            LoadLastProfiles();

            int index = lastProfiles.FindIndex((lp) => { return lp.Id == id; });
            if (index < 0)
                lastProfiles.Add(new LastProfile { Id = id, Name = name });
            else
            {
                LastProfile lastProfile = lastProfiles[index];
                lastProfile.Name = name;
                lastProfiles[index] = lastProfile;
            }

            File.WriteAllText(lastProfilesPath, JsonSerializer.Serialize(lastProfiles));
        }
        public static string GetLastProfile(string id)
        {
            LoadLastProfiles();
            int index = lastProfiles.FindIndex((lp) => { return lp.Id == id; });
            if (index < 0)
                return string.Empty;
            else
                return lastProfiles[index].Name;
        }
#pragma warning restore CS8602 // Desreferencia de una referencia posiblemente NULL.
    }
}
