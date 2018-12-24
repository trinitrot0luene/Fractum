using System.Threading.Tasks;
using Fractum;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class ResumedEventHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, GatewaySession session)
        {
            cache.Client.InvokeLog(new LogMessage(nameof(ResumedEventHook), "Resumed", LogSeverity.Info));

            return Task.CompletedTask;
        }
    }
}