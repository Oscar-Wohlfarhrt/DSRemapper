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
        private readonly CancellationToken cancellationToken;
        //Timer timer;

        public Remapper(IDSInputController controller, IDSRemapper remapper)
        {
            this.controller = controller;
            this.remapper = remapper;
            cancellationToken = new CancellationToken();

            /*timer = new Timer(RemapTimer);
            timer.Change(0,10);*/
            
            thread = new(RemapThread)
            {
                Name=$"{controller.ControllerName} Remapper",
                Priority = ThreadPriority.Normal
            };

            /*Task.Factory.StartNew(RemapThread, CancellationToken.None,
                TaskCreationOptions.LongRunning, TaskScheduler.Default);*/
        }

        public void Start()
        {
            thread.Start();
        }

        public void Stop()
        {
            
        }

        private void RemapThread()
        {
            while (cancellationToken.IsCancellationRequested)
            {

                Thread.Sleep(10);
            }
        }
        private void RemapTimer(object? sender)
        {

        }
    }
}