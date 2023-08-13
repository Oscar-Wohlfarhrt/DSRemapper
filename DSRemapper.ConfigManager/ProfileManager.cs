using DSRemapper.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSRemapper.ConfigManager
{
    /// <summary>
    /// This class manage all remap profiles
    /// </summary>
    public static class ProfileManager
    {
        /// <summary>
        /// Get all the files inside the DSRemapper plugins folder and subfolders
        /// </summary>
        /// <returns>An array containing all remap profile files</returns>
        public static string[] GetProfiles() =>
            Directory.GetFiles(DSPaths.ProfilesPath, "*.*", SearchOption.AllDirectories)
            .Select((f) => Path.GetRelativePath(DSPaths.ProfilesPath, f)).ToArray();
    }
}
