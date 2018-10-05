using Fractum.WebSocket.Entities;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.WebSocket.Pipelines;

namespace Fractum.WebSocket.Hooks
{
    public sealed class GuildCreateHook : IEventHook<JToken>
    {
        public async Task RunAsync(JToken data, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var guild = data.ToObject<GuildCreateModel>();

            cache.AddGuild(guild);

            if (client.Config.AlwaysDownloadMembers)
                await client.RequestMembersAsync(guild.Id);

            client.InvokeLog(new LogMessage(nameof(GuildCreateHook), $"Guild Available: {guild.Name}", LogSeverity.Info));

            return;
        }
    }
}
