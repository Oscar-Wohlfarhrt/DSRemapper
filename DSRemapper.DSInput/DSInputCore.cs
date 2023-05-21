using DSRemapper.Core;
using System.Reflection;

namespace DSRemapper.DSInput
{
    public class DSInputCore
    {
        public static void LoadAnsemblies()
        {
            Assembly a = Assembly.LoadFile(Path.Combine(Core.Core.DSFolderPath, "DSRemapper.DualShock.dll"));
            Type[] ts = a.GetTypes().Where((t) => { return t.IsAssignableTo(typeof(ITest)); }).ToArray();
            foreach (Type t in ts)
            {
                Console.WriteLine(t.FullName);
            }
        }

        public interface ITest
        {
            public string GetName();
        }
    }
}