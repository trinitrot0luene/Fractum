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
    class GuildMemberChunkHook : IEventHook<JToken>
    {
        public Task RunAsync(JToken args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var guildMemberChunkEvent = args.ToObject<GuildMemberChunkEvent>();
            guildMemberChunkEvent.ApplyToCache(cache);

            client.InvokeLog(new LogMessage(nameof(GuildMemberChunkHook), $"Received a {guildMemberChunkEvent.Members.Count} member chunk for guild {cache.GetGuild(guildMemberChunkEvent.GuildId).Name}", LogSeverity.Debug));
            return Task.CompletedTask;
        }
    }
}
