using DSRemapper.Types;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DSRemapper.Core
{
    public class Core
    {

    }

    [AttributeUsage(AttributeTargets.Class)]
    public class EmulatedControllerAttribute : Attribute
    {
        public string DevicePath { get; set; }
        public bool IsGlobal { get; set; }
        public EmulatedControllerAttribute(string path, bool isGlobal=false)
        {
            DevicePath = path;
            IsGlobal = isGlobal;
        }
    }
    public enum RemapperEventType
    {
        DeviceConsole,
        Warning,
        Error
    }
    public delegate void RemapperEventArgs(RemapperEventType type, string message);
    [AttributeUsage(AttributeTargets.Class)]
    public class RemapperAttribute : Attribute
    {
        public string FileExt { get; set; }
        public RemapperAttribute(string fileExt)
        {
            FileExt = fileExt;
        }
    }

    #region Interfaces

    public interface IDSInputDeviceInfo
    {
        public string Id { get; }
        public string Name { get; }
        public int VendorId { get; }
        public int ProductId { get; }
        public IDSInputController CreateController();
        public virtual string ToString() => $"Device {Name} [{Id}]";
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
        /// <summary>
        /// Implementation for custom user defined functions.
        /// Created to implement needed functions not suported by the interface.
        /// Override for custom implementation, default implementation use reflection.
        /// </summary>
        /// <param name="funcName">Name of custom function</param>
        /// <param name="args">Optional arguments for the custom function</param>
        public virtual void CustomFunc(string funcName, params object[]? args)
        {
            MethodInfo? method = GetType().GetMethod(funcName,
                args == null ? Type.EmptyTypes : args.Select(a => a.GetType()).ToArray());
            method?.Invoke(this, args);
        }
    }

    public interface IDSRemapper : IDisposable
    {
        public event RemapperEventArgs? OnLog;
        public void SetScript(string file);
        public DSOutputReport Remap(DSInputReport report);
    }
    #endregion Interfaces

    #region Structures
    
    #endregion Structures
}