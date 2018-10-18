using System;
using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.Entities.Extensions;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;
using Newtonsoft.Json.Linq;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class PresenceUpdateHook : IEventHook<JToken>
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