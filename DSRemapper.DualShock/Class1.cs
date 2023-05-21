using static DSRemapper.DSInput.DSInputCore;

namespace DSRemapper.DualShock
{
    public class Test1 : ITest
    {
        public string GetName() => "Hello World";
    }
    public class Test2
    {
        public string GetName() => "Hello World";
    }
    namespace Test
    {
        public class Test3 : ITest
        {
            public string GetName() => "Hello World";
        }
    }
}