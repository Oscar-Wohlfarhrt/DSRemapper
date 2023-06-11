using DSRemapper.Core;
using DSRemapper.Types;

namespace DSRemapper.COMM
{
    public class COMM : IDSInputController
    {
        public string Id => throw new NotImplementedException();

        public string Name => throw new NotImplementedException();

        public string Type => throw new NotImplementedException();

        public bool IsConnected => throw new NotImplementedException();

        public void Connect()
        {
            throw new NotImplementedException();
        }

        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void ForceDisconnect()
        {
            throw new NotImplementedException();
        }

        public DSInputReport GetInputReport()
        {
            throw new NotImplementedException();
        }

        public void SendOutputReport(DSOutputReport report)
        {
            throw new NotImplementedException();
        }
    }
}