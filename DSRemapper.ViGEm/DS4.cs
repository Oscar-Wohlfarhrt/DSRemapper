using DSRemapper.Core;
using DSRemapper.Types;

namespace DSRemapper.ViGEm
{
    public class DS4 : IDSOutputController
    {
        public bool IsConnected => throw new NotImplementedException();

        public DSInputReport state => throw new NotImplementedException();

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

        public DSOutputReport GetFeedbackReport()
        {
            throw new NotImplementedException();
        }

        public void Update()
        {
            throw new NotImplementedException();
        }
    }
}