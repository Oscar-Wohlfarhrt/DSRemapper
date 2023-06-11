using DSRemapper.Core;
using DSRemapper.Types;

namespace DSRemapper.RemapperLua
{
    public class LuaInterpreter : IDSRemapper
    {
        public IDSInputController Controller { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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

        public DSOutputReport Remap(DSInputReport report)
        {
            throw new NotImplementedException();
        }
    }
}