using DSRemapper.Types;
using System.Reflection;

namespace DSRemapper.Core
{
    public class Core
    {

    }

    #region Interfaces

    public abstract class IDSInputDeviceInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        
        public IDSInputDeviceInfo(string id, string name, string description="none")
        {
            Id = id;
            Name = name;
            Description = description;
        }
        public abstract IDSInputController CreateController();
        public override string ToString() => $"Device {Name} [{Id}]";
    }
    public interface IDSDeviceScanner 
    {
        public IDSInputDeviceInfo[] ScanDevices();
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