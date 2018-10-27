using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    public class ResumedEventHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, ISocketCache<ISyncedGuild> cache, ISession session)
        {
            cache.Client.InvokeLog(new LogMessage(nameof(ResumedEventHook), "Resumed", LogSeverity.Info));

            return Task.CompletedTask;
        }
    }
}