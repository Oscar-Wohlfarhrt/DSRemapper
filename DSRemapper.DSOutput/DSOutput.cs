using DSRemapper.ConfigManager;
using DSRemapper.Core;
using DSRemapper.Types;
using System.Reflection;

namespace DSRemapper.DSOutput
{
    /// <summary>
    /// DSRemapper class that amange emulated controllers for remapper plugins
    /// </summary>
    public class DSOutput : IDisposable
    {
        private List<IDSOutputController> emulatedControllers = new();
        private static readonly Dictionary<string,SharedController> sharedControllers = new();
        /// <summary>
        /// Creates a standalone emulated controller for the remapper.
        /// </summary>
        /// <param name="path">Path/id of the emulated controller type (for example: "ViGEm/DS4" for a ViGEm plugin Dualshock 4 controller)</param>
        /// <returns>An interface of the emulated controller</returns>
        /// <exception cref="Exception">The controller path/id doesn't exist and can't be found</exception>
        public IDSOutputController CreateController(string path)
        {
            if (PluginsLoader.OutputPlugins.TryGetValue(path, out ConstructorInfo? plugin))
            {
                IDSOutputController controller = (IDSOutputController)plugin.Invoke(null);
                emulatedControllers.Add(controller);
                return controller;
            }

            throw new Exception($"{path}: Emulated controller not found");
        }
        /// <summary>
        /// Creates a global emulated controller, which can be shared with multiple remap profiles for complex setups.
        /// If a emulated controller of the type and id is currently defined, the function returns the controller binded to the id.
        /// Be careful setting the same fields of the controller state from multiple remap profiles, can lead to errors.
        /// </summary>
        /// <param name="id">The id of the binded controller</param>
        /// <param name="path">Path/id of the emulated controller type (for example: "ViGEm/DS4" for a ViGEm plugin Dualshock 4 controller)</param>
        /// <returns>An interface of the emulated controller</returns>
        public IDSOutputController GetController(string id,string path)
        {
            string fullid = $"{path};{id}";
            if (!sharedControllers.TryGetValue(fullid, out SharedController? ctrl))
            {
                ctrl = new SharedController(CreateController(path),this);
                sharedControllers.Add(fullid, ctrl);
            }

            ctrl.Connect(this);
            return ctrl.Controller;

        }
        /// <summary>
        /// Disconnects ALL emulated controllers (standalone and shared ones)
        /// </summary>
        public void DisconnectAll()
        {
            foreach (var controller in emulatedControllers)
            {
                controller.Disconnect();
                controller.Dispose();
            }
            DisconnectAllBinded();
            emulatedControllers = new();
        }
        /// <summary>
        /// Disconnects THIS DSOutput object from all shared controllers
        /// </summary>
        public void DisconnectAllBinded()
        {
            foreach (string key in sharedControllers.Keys)
            {
                if(sharedControllers.TryGetValue(key,out SharedController? ctrl))
                {
                    ctrl.Disconnect(this);
                    if (ctrl.Count<=0) {
                        sharedControllers.Remove(key);
                        ctrl.Dispose();
                    }
                }
            }
        }
        /// <inheritdoc/>
        public void Dispose()
        {
            DisconnectAll();
            GC.SuppressFinalize(this);
        }
    }
    /// <summary>
    /// Container class for shared emulated controllers
    /// </summary>
    public class SharedController : IDisposable
    {
        /// <summary>
        /// The controller to share between remap profiles
        /// </summary>
        public IDSOutputController Controller { get; private set; }
        /// <summary>
        /// All object references binded to this controller
        /// </summary>
        private readonly List<object> references = new();
        /// <summary>
        /// Count of binded objects
        /// </summary>
        public int Count => references.Count;
        /// <summary>
        /// Shared Controller class constructor
        /// </summary>
        /// <param name="controller">The emulated controller for the shared container</param>
        /// <param name="firstReference">The first object which is binded to the shared controller</param>
        public SharedController(IDSOutputController controller,object firstReference)
        {
            Connect(firstReference);
            Controller = controller;
            controller.Connect();
        }
        /// <summary>
        /// Connects/binds new object reference to the controller
        /// </summary>
        /// <param name="reference">Object to bind to this shared controller</param>
        public void Connect(object reference)
        {
            if (!references.Contains(reference))
                references.Add(reference);
        }
        /// <summary>
        /// Disconnects/unbinds object reference to the controller
        /// </summary>
        /// <param name="reference">Object to bind to this shared controller</param>
        public void Disconnect(object reference)
        {
            if(references.Contains(reference))
                references.Remove(reference);
        }
        /// <summary>
        /// Checks if the object is referenced to the shared controller
        /// </summary>
        /// <param name="reference">Object supposedly binded to this shared controller</param>
        /// <returns>True if the object is currently binded to the controller, otherwise false</returns>
        public bool IsReferenced(object reference) => references.Contains(reference);
        /// <inheritdoc/>
        public void Dispose()
        {
            Controller.Disconnect();
            Controller.Dispose();
            GC.SuppressFinalize(this);
        }

    }
}