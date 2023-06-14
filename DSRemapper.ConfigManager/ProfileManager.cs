using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSRemapper.ConfigManager
{
    public static class ProfileManager
    {
        public static string[] GetProfiles()
        {
            return Directory.GetFiles(DSPaths.ProfilesPath, "*.*", SearchOption.AllDirectories)
                .Select((f) => Path.GetRelativePath(DSPaths.ProfilesPath, f)).ToArray();
        }
    }
}
