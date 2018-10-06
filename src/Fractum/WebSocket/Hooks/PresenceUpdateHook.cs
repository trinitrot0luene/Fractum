using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;
using Fractum.WebSocket.Pipelines;
using Newtonsoft.Json.Linq;

namespace Fractum.WebSocket.Hooks
{
    public sealed class PresenceUpdateHook : IEventHook<JToken>
    {
        public Task RunAsync(JToken args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var presenceUpdate = args.ToObject<PresenceUpdateEventModel>();

            presenceUpdate.ApplyToCache(cache);

            client.InvokeLog(new LogMessage(nameof(PresenceUpdateHook), "Presence Update", LogSeverity.Debug));

            return Task.CompletedTask;
        }
    }
}