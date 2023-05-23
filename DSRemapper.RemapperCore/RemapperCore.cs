using DSRemapper.Core;

namespace DSRemapper.RemapperCore
{
    public class RemapperCore
    {

    }

    public class Remapper
    {
        private readonly IDSInputController controller;
        private readonly IDSRemapper remapper;
        private readonly Thread thread;

        public Remapper(IDSInputController controller, IDSRemapper remapper)
        {
            this.controller = controller;
            this.remapper = remapper;
            thread = new();
        }
    }
}