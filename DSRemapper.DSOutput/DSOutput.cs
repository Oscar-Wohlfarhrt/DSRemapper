using DSRemapper.ConfigManager;
using DSRemapper.Core;
using DSRemapper.Types;
using System.Reflection;

namespace DSRemapper.DSOutput
{
    public class DSOutput
    {
        private List<IDSOutputController> emulatedControllers = new();
        static Dictionary<string,BindedController> bindedControllers = new();

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
        public IDSOutputController GetController(string id,string path)
        {
            string fullid = $"{path};{id}";
            if (!bindedControllers.TryGetValue(fullid, out BindedController? ctrl))
            {
                ctrl = new BindedController(CreateController(path),this);
                bindedControllers.Add(fullid, ctrl);
            }

            ctrl.Connect(this);
            return ctrl.Controller;

        }
        public void DisconnectAll()
        {
            foreach (var controller in emulatedControllers)
            {
                controller.Disconnect();
                controller.Dispose();
            }
            DisconnectAllBinded();
        }
        public void DisconnectAllBinded()
        {
            foreach (string key in bindedControllers.Keys)
            {
                if(bindedControllers.TryGetValue(key,out BindedController? ctrl))
                {
                    ctrl.Disconnect(this);
                    if (ctrl.Count<=0) {
                        bindedControllers.Remove(key);
                        ctrl.Dispose();
                    }
                }
            }
        }
    }

    public class BindedController : IDisposable
    {
        public IDSOutputController Controller { get; private set; }
        private List<object> references = new();
        public int Count => references.Count;

        public BindedController(IDSOutputController controller,object firstReference)
        {
            Connect(firstReference);
            Controller = controller;
            controller.Connect();
        }
        public void Connect(object reference)
        {
            if (!references.Contains(reference))
                references.Add(reference);
        }

        public void Disconnect(object reference)
        {
            if(references.Contains(reference))
                references.Remove(reference);
        }
        public bool IsReferenced(object reference) => references.Contains(reference);

        public void Dispose()
        {
            Controller.Disconnect();
            Controller.Dispose();
        }

    }
}