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
        public virtual string Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Path { get; set; }
        public virtual int VendorId { get; set; }
        public virtual int ProductId { get; set; }

        public IDSInputDeviceInfo(string path, string name, string id, int vendorId, int productId)
        {
            Path = path;
            Id = id;
            Name = name;
            VendorId = vendorId;
            ProductId = productId;
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
        public string Name { get; }
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
        public DSOutputReport Remap(DSInputReport report);
    }
    #endregion Interfaces

    #region Structures
    
    #endregion Structures
}