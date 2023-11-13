namespace Backend.Core.PubSub
{
    public interface IBroadcastService
    {
        Task Propagate<T>(T value);
        Task Receive<T>(Action<T> action);
    }
}
