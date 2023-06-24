using DSRemapper.ConfigManager;
using DSRemapper.Core;

namespace DSRemapper.DSOutput
{
    public class DSOutput
    {
        public IDSOutputController CreateController(string path)
        {
            if(PluginsLoader.OutputPlugins.TryGetValue(path, out var plugin)) { }
            return plugin.Invoke(null);
        }
    }
}