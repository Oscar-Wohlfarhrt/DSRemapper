using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DSRemapper
{
    [ComVisible(true)]
    internal class DSRBridge
    {
        public void LogOnConsole(string str)
        {
            Console.WriteLine(str);
        }
    }
}
