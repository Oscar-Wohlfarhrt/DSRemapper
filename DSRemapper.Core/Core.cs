using DSRemapper.Types;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DSRemapper.Core
{
    /// <summary>
    /// Enumeration of the Remappers message types
    /// </summary>
    public enum RemapperEventType
    {
        /// <summary>
        /// Shows the message to the device console of the program
        /// </summary>
        DeviceConsole,
        /// <summary>
        /// Shows the message as a warning message on the debug console of the program
        /// </summary>
        Warning,
        /// <summary>
        /// Shows the message as a error message on the debug console of the program
        /// </summary>
        Error
    }
    /// <summary>
    /// Delegate for Remappers message events
    /// </summary>
    /// <param name="type">The type of the current message</param>
    /// <param name="message">A string containing the message sent by the Remapper object</param>
    public delegate void RemapperEventArgs(RemapperEventType type, string message);
    /// <summary>
    /// Attribute to bind Remappers to their corresponding file extensions
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RemapperAttribute : Attribute
    {
        /// <summary>
        /// The extension of the file referenced by this attribute
        /// </summary>
        public string FileExt { get; set; }
        /// <summary>
        /// RemapperAttribute class contructor
        /// </summary>
        /// <param name="fileExt">The file extension (without the dot) binded to the Remapper class</param>
        public RemapperAttribute(string fileExt)
        {
            FileExt = fileExt;
        }
    }
    /// <summary>
    /// An attribute to reference a emulated controller with a unique id/path to instance it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class EmulatedControllerAttribute : Attribute
    {
        /// <summary>
        /// Gets/Sets the virtual device id/path
        /// </summary>
        public string DevicePath { get; set; }
        /// <summary>
        /// Gets/Sets if the controller is global or not
        /// </summary>
        public bool IsGlobal { get; set; }
        /// <summary>
        /// EmulatedControllerAttribute class contructor
        /// </summary>
        /// <param name="path">The id/path for the emulated controller</param>
        /// <param name="isGlobal">If is true the emulated controller will not be referenced to a specific Remapper (default value: false) [still not implemented]</param>
        public EmulatedControllerAttribute(string path, bool isGlobal = false)
        {
            DevicePath = path;
            IsGlobal = isGlobal;
        }
    }

    #region Interfaces
    /// <summary>
    /// A standard interface for devices infos handled by DSRemapper
    /// </summary>
    public interface IDSInputDeviceInfo
    {
        /// <summary>
        /// Unique id for the device referenced by this interface.
        /// </summary>
        public string Id { get; }
        /// <summary>
        /// Friendly name for the device referenced by this interface.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Creates a IDSInputController that references the physical controller referenced by this interface.
        /// Works as a shortcut to the corresponding controller constructor to simplify the program code.
        /// </summary>
        /// <returns>A initialized IDSInputController object</returns>
        public IDSInputController CreateController();
        /// <summary>
        /// Returns de device name and id as a string with a default format.
        /// </summary>
        /// <returns>A string with device name and id</returns>
        public virtual string? ToString() => $"Device {Name} [{Id}]";
    }
    /// <summary>
    /// A standard Device Scanner interface for DSRemapper
    /// </summary>
    public interface IDSDeviceScanner
    {
        /// <summary>
        /// Returns an array with the information of connected devices for a specific plugin or controller type.
        /// </summary>
        /// <returns>An array of IDSInputDeviceInfo objects</returns>
        public IDSInputDeviceInfo[] ScanDevices();
    }
    /// <summary>
    /// Standard input controller of DSRemapper
    /// </summary>
    public interface IDSInputController : IDisposable
    {
        /// <summary>
        /// Gets the controller Id
        /// </summary>
        public string Id { get; }
        /// <summary>
        /// Gets the controller name
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Gets the controller type
        /// </summary>
        public string Type { get; }
        /// <summary>
        /// Gets if the controller is currently connected
        /// </summary>
        public bool IsConnected { get; }
        /// <summary>
        /// Connects the controller to start reading and writing from/to it
        /// </summary>
        public void Connect();
        /// <summary>
        /// Disconnects the controller from DSRemapper
        /// </summary>
        public void Disconnect();
        /// <summary>
        /// Gets the input state of the controller, which includes axes positions, buttons, etc.
        /// </summary>
        /// <returns>A standard DSRemapper input report</returns>
        public DSInputReport GetInputReport();
        /// <summary>
        /// Sets the output state of a controller sending information about vibration, force feedback, etc.
        /// </summary>
        /// <param name="report">A standard DSRemapper output report with the information for the controller</param>
        public void SendOutputReport(DSOutputReport report);
    }
    /// <summary>
    /// Standard interface for DSRemapper emulated controllers
    /// </summary>
    public interface IDSOutputController : IDisposable
    {
        /// <summary>
        /// Gets if the emulated controller is connected and updating it's data
        /// </summary>
        public bool IsConnected { get; }
        /// <summary>
        /// Gets the state structure of the emulated controller
        /// </summary>
        public DSInputReport State { get; }
        /// <summary>
        /// Default Connect function to connect the emulated controller
        /// </summary>
        public void Connect();
        /// <summary>
        /// Disconnects the emulated controller
        /// </summary>
        public void Disconnect();
        /// <summary>
        /// Updates the emulated controller values using the state property values
        /// </summary>
        public void Update();
        /// <summary>
        /// Gets the current state of the feedback sended to the emulated controller from the computer
        /// </summary>
        /// <returns>A standard DSRemapper output report</returns>
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
    /// <summary>
    /// Standard interface for 
    /// </summary>
    public interface IDSRemapper : IDisposable
    {
        /// <summary>
        /// OnLog event to comunicate Remapper messages
        /// </summary>
        public event RemapperEventArgs? OnLog;
        /// <summary>
        /// Sets the script for the remapper setting it up to start remapping the controller
        /// </summary>
        /// <param name="file">File path to the Remap Profile file</param>
        public void SetScript(string file);
        /// <summary>
        /// Main remap function of a Remapper class. This funciton is called every time the program needs to update the emulated controllers.
        /// </summary>
        /// <param name="report">Standard DSRemapper input report with the state of physical controller</param>
        /// <returns>Standard DSRemapper output report with the feedback state for the physical controller</returns>
        public DSOutputReport Remap(DSInputReport report);
    }
    #endregion Interfaces
}