using Fractum.Entities;
using Fractum.WebSocket.Events;
using Fractum.WebSocket.Pipelines;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fractum.WebSocket.Hooks
{
    public sealed class PresenceUpdateHook : IEventHook<JToken>
    {
        public Task RunAsync(JToken args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var presenceUpdate = args.ToObject<PresenceUpdateEvent>();

            presenceUpdate.ApplyToCache(cache);

            client.InvokeLog(new LogMessage(nameof(PresenceUpdateHook), "Presence Update", LogSeverity.Verbose));

            return Task.CompletedTask;
        }
    }
}
