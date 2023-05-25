using DSRemapper.Core;
using DSRemapper.DSLogger;
using DSRemapper.Types;
using System.Management;

namespace DSRemapper.DualShock
{
    public class DualShockInfo : IDSInputDeviceInfo
    {
        public DualShockInfo(string path, string name, string id, int vendorId, int productId, string description = "none")
            : base(path, name, id, vendorId, productId, description){}

        public override IDSInputController CreateController()
        {
            return new DualShock();
        }
    }

    public class DualShockScanner : IDSDeviceScanner
    {
        public IDSInputDeviceInfo[] ScanDevices()
        {
            //VID&0002054C
            //VID&0002054C_PID&09CC
            int vid = 0x054C, pid = 0x0000;//#{4d1e55b2-f16f-11cf-88cb-001111000030}
            string wmiQuery = @$"SELECT DeviceID FROM Win32_PnPEntity WHERE DeviceID like 'HID%VID%{(vid>0? $"{vid:X4}":"")}_PID_{(pid > 0 ? $"{pid:X4}" : "")}%'";

            ManagementObjectSearcher searcher = new ManagementObjectSearcher(wmiQuery);
            ManagementObjectCollection objCollection = searcher.Get();
            Logger.Log("Path: \\\\?\\hid#{00001124-0000-1000-8000-00805f9b34fb}_vid&0002054c_pid&09cc#9&2005301&25&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}");
            foreach (var obj in objCollection)
            {
                
                Logger.Log($"Path: \\\\?\\{obj["DeviceID"].ToString().Replace("\\","#")}#{{4d1e55b2-f16f-11cf-88cb-001111000030}}");
            }
            return new[]{
                new DualShockInfo("path","name","id",0,0)
            };
        }
    }
    public class DualShock : IDSInputController
    {
        public string Id => throw new NotImplementedException();

        public string ControllerName => throw new NotImplementedException();

        public string Type => throw new NotImplementedException();

        public bool IsConnected => throw new NotImplementedException();

        public DualShock()
        {

        }
        public void Connect()
        {
            throw new NotImplementedException();
        }

        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void ForceDisconnect()
        {
            throw new NotImplementedException();
        }

        public DSInputReport GetInputReport()
        {
            throw new NotImplementedException();
        }

        public void SendOutputReport(DSOutputReport report)
        {
            throw new NotImplementedException();
        }
    }
}