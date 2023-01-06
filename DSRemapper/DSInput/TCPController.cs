using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSRemapper.DSInput
{
    internal class TCPController
    {
        public string Id => "";
        public string ControllerName => "";
        public ControllerType Type => ControllerType.TCP;
        public bool IsConnected => false;

        public void Connect()
        {

        }
        public void Disconnect()
        {

        }
        public void Dispose()
        {

        }
        public DSInputReport GetInputReport()
        {
            return new DSInputReport();
        }
        public void SendOutputReport(DSOutputReport report)
        {

        }
    }
}
