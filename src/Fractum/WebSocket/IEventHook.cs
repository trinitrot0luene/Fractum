using System.Threading.Tasks;
using Fractum;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket
{
    public interface IEventHook<in TArgs> where TArgs : EventModelBase
    {
        Task RunAsync(TArgs args, GatewayCache cache, GatewaySession session);
    }
}