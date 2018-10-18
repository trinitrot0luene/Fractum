using System.Threading.Tasks;
using Fractum.WebSocket;
using Fractum.WebSocket.Core;

namespace Fractum.Contracts
{
    public interface IEventHook<TArgs>
    {
        Task RunAsync(TArgs args, FractumCache cache, ISession session, FractumSocketClient client);
    }
}