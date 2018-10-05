using System.Threading.Tasks;

namespace Fractum.WebSocket.Pipelines
{
    public interface IEventHook<TArgs>
    {
        Task RunAsync(TArgs args, FractumCache cache, ISession session, FractumSocketClient client);
    }
}