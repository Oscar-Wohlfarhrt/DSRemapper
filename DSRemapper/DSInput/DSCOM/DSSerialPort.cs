using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace DSRemapper.DSInput.DSCOM
{
    public struct SerialDeviceInfo
    {
        public string PortName { get; set; }
        public string DeviceName { get; set; }

        public SerialDeviceInfo(string portName, string devName)
        {
            PortName = portName;
            DeviceName = devName;
        }
        public override string ToString()
        {
            return $"Name: {DeviceName} / Port: {PortName}";
        }
    }

    internal class DSSerialPort
    {

        private readonly SerialPort sp;

        public string PortName { get => sp.PortName; }
        public int BaudRate { get => sp.BaudRate;set=> sp.BaudRate = value; }
        public string DeviceName { get; private set; }

        public DSSerialPort(SerialDeviceInfo devInfo, int baudRate=9600) : this(devInfo.PortName,baudRate,devInfo.DeviceName) { }
        public DSSerialPort(string portName, int baudRate = 9600, string deviceName = "Unknown")
        {
            sp = new SerialPort(portName, baudRate);
            DeviceName = deviceName;
        }

        public static SerialDeviceInfo[] GetSerialDevices()
        {
            string wmiQuery = @"SELECT DeviceID, Name FROM Win32_SerialPort";

            ManagementObjectSearcher searcher = new ManagementObjectSearcher(wmiQuery);
            ManagementObjectCollection objCollection = searcher.Get();

            List<SerialDeviceInfo> infos = new();

            foreach (ManagementObject obj in objCollection)
            {
                string? port = obj["DeviceID"].ToString();
                string? name = obj["Name"].ToString();

                if (port != null && name != null)
                    infos.Add(new(port, name));
            }

            return infos.ToArray();
        }        
    }
}