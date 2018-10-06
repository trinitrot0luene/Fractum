using System.Threading.Tasks;
using Fractum.WebSocket.Core;

namespace Fractum.WebSocket.Pipelines
{
    public interface IEventHook<TArgs>
    {
        Task RunAsync(TArgs args, FractumCache cache, ISession session, FractumSocketClient client);
    }
}