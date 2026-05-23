namespace NanoFrameworkApp.Workers
{
    public interface IWorker
    {
        void Start();
        void Stop();
        string Name { get; }
        bool IsRunning { get; }
    }
}
