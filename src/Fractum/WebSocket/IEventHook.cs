using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket
{
    public interface IEventHook<in TArgs> where TArgs : EventModelBase
    {
        Task RunAsync(TArgs args, FractumCache cache, GatewaySession session);
    }
}