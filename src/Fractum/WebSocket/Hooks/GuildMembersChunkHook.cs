using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.WebSocket.Events;
using Fractum.WebSocket.Pipelines;
using Newtonsoft.Json.Linq;

namespace Fractum.WebSocket.Hooks
{
    internal class GuildMembersChunkHook : IEventHook<JToken>
    {
        public Task RunAsync(JToken args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var guildMemberChunkEvent = args.ToObject<GuildMemberChunkEvent>();
            guildMemberChunkEvent.ApplyToCache(cache);

            client.InvokeLog(new LogMessage(nameof(GuildMembersChunkHook),
                $"Received a {guildMemberChunkEvent.Members.Count} member chunk for guild {cache.GetGuild(guildMemberChunkEvent.GuildId).Name}",
                LogSeverity.Debug));
            return Task.CompletedTask;
        }
    }
}