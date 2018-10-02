using Fractum.WebSocket.Pipelines;
using Fractum.WebSocket;
using Fractum.WebSocket.Entities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Fractum.Entities;

namespace Fractum.WebSocket.Hooks
{
    public sealed class GuildCreateHook : IEventHook<JToken>
    {
        public Task RunAsync(JToken data, IFractumCache cache, ISession session, FractumSocketClient client)
        {
            var guild = data.ToObject<Guild>();

            cache.Guilds.AddOrUpdate(guild.Id, guild, (k, v) => guild ?? v);

            client.InvokeLog(new LogMessage(nameof(GuildCreateHook), $"Guild Available: {guild.Name}", LogSeverity.Info));

            return Task.CompletedTask;
        }
    }
}
