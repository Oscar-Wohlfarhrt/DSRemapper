using DSRemapper.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DSRemapper
{
    public class DSRBridge
    {
        public string test { get => "Hello JS!"; }
        public void LogOnConsole(string str)
        {
            Console.WriteLine(str);
        }
        public string LoopBack(string str)
        {
            return str;
        }

        public IDSInputDeviceInfo[] GetDevicesInfo() => DSInput.DSInput.GetDevicesInfo();
    }
}
