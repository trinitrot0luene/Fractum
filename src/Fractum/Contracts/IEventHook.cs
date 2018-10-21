using System.Threading.Tasks;
using Fractum.WebSocket;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;

namespace Fractum.Contracts
{
    public interface IEventHook<TArgs> where TArgs : EventModelBase
    {
        Task RunAsync(TArgs args, FractumCache cache, ISession session, FractumSocketClient client);
    }
}