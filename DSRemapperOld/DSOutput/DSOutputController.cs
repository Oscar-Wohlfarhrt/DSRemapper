using DSRemapper.DSInput;
using MoonSharp.Interpreter.Interop;
using Nefarius.ViGEm.Client;

namespace DSRemapper.DSOutput
{
    public interface IDSOutputController : IDisposable
    {
        public bool IsConnected { get; }
        public DSInputReport state { get; }
        public void Connect();
        public void Disconnect();
        public void Update();
        public void SetInputReport(DSInputReport report);
        public DSOutputReport GetFeedbackReport();
    }
    public class DSOutputController
    {
        private static ViGEmClient vigem = new();

        private List<IDSOutputController> controllers = new();

        [MoonSharpVisible(false)]
        public void DisconnectAll()
        {
            foreach (IDSOutputController controller in controllers)
            {
                controller.Dispose();
            }
            controllers.Clear();
        }
        public IDSOutputController CreateDS4()=> AddController(new DSEmulated(vigem.CreateDualShock4Controller()));
        public IDSOutputController CreateXbox() => AddController(new XboxEmulated(vigem.CreateXbox360Controller(0x045E, 0x0280)));
        public IDSOutputController CreateVJoy(uint id,byte buttons=32, uint axisRange = 32768) => AddController(new VJEmulated(id,buttons, axisRange));
        private IDSOutputController AddController(IDSOutputController ctrl)
        {
            controllers.Add(ctrl);
            return ctrl;
        }
    }

    internal static class FloatExtensions
    {
        public static short ToShortAxis(this float axis) => (short)(axis * (axis < 0 ? -short.MinValue : short.MaxValue));
        public static sbyte ToSByteAxis(this float axis) => (sbyte)(axis * (axis < 0 ? -sbyte.MinValue : sbyte.MaxValue));
        public static byte ToByteTrigger(this float axis) => (byte)(axis * byte.MaxValue);
    }
}
