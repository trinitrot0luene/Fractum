using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;
using Fractum.WebSocket.Pipelines;
using Newtonsoft.Json.Linq;

namespace Fractum.WebSocket.Hooks
{
    public sealed class GuildCreateHook : IEventHook<JToken>
    {
        public async Task RunAsync(JToken data, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var guild = data.ToObject<GuildCreateEventModel>();

            guild.ApplyToCache(cache);

            if (client.Config.AlwaysDownloadMembers)
                await client.RequestMembersAsync(guild.Id);

            client.InvokeLog(
                new LogMessage(nameof(GuildCreateHook), $"Guild Available: {guild.Name}", LogSeverity.Info));

            client.InvokeGuildCreated(cache.GetGuild(guild.Id));
        }
    }
}