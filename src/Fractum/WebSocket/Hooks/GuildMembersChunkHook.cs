using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;
using Newtonsoft.Json.Linq;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class GuildMembersChunkHook : IEventHook<JToken>
    {
        public Task RunAsync(JToken args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var guildMemberChunkEvent = args.ToObject<GuildMemberChunkEventModel>();
            guildMemberChunkEvent.ApplyToCache(cache);

            client.InvokeLog(new LogMessage(nameof(GuildMembersChunkHook),
                $"Received a {guildMemberChunkEvent.Members.Count} member chunk for guild {cache[guildMemberChunkEvent.GuildId].Guild.Name}",
                LogSeverity.Debug));
            return Task.CompletedTask;
        }
    }
}