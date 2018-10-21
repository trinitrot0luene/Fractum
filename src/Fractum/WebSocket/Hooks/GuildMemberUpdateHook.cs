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
    internal sealed class GuildMemberUpdateHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var presenceUpdate = args.Cast<GuildMemberUpdateEventModel>();

            presenceUpdate.ApplyToCache(cache);

            client.InvokeLog(new LogMessage(nameof(GuildMemberUpdateHook), "Presence Update", LogSeverity.Debug));

            return Task.CompletedTask;
        }
    }
}