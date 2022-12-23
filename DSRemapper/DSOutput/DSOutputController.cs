using DSRemapper.DSInput;
using Nefarius.ViGEm.Client;

namespace DSRemapper.DSOutput
{
    public interface IDSOutputController : IDisposable
    {
        public bool IsConnected { get; }
        public void Connect();
        public void Disconnect();
        public void SetInputReport(DSInputReport report);
        public DSOutputReport GetFeedbackReport();
    }
    public class DSOutputController
    {
        private static ViGEmClient vigem = new();

        private List<IDSOutputController> controllers = new();
        public void DisconnectAll()
        {
            foreach (IDSOutputController controller in controllers)
            {
                controller.Dispose();
            }
            controllers.Clear();
        }
        public IDSOutputController CreateDS4Controller()=> AddController(new DSEmulated(vigem.CreateDualShock4Controller()));
        public IDSOutputController CreateXboxController() => AddController(new XboxEmulated(vigem.CreateXbox360Controller(0x045E, 0x0280)));
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
