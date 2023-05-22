using DSRemapper.Types;
using System.Reflection;

namespace DSRemapper.Core
{
    public class Core
    {

    }

    public class DSInputDeviceInfo<T> where T : IDSInputController,new()
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public DSInputDeviceInfo(string id, string name) : this(id, name, "") { }
        public DSInputDeviceInfo(string id,string name,string description) {
            Id = id;
            Name = name;
            Description = description;
        }

        public T CreateController()
        {
            return new T();
        }

        public override string ToString()
        {
            return $"Device {Id} - {Name}";
        }
    }

    #region Interfaces
    public interface IDSDeviceScanner 
    {
        public DSInputDeviceInfo<IDSInputController>[] ScanDevices();
    }
    public interface IDSInputController : IDisposable
    {
        public string Id { get; }
        public string ControllerName { get; }
        public string Type { get; }
        public bool IsConnected { get; }
        public void Connect();
        public void Disconnect();
        public void ForceDisconnect();
        public DSInputReport GetInputReport();
        public void SendOutputReport(DSOutputReport report);
    }

    public interface IDSOutputController : IDisposable
    {
        public bool IsConnected { get; }
        public DSInputReport state { get; }
        public void Connect();
        public void Disconnect();
        public void Update();
        public DSOutputReport GetFeedbackReport();
    }

    public interface IDSRemapper : IDisposable
    {

    }
    #endregion Interfaces

    #region Structures
    
    #endregion Structures
}