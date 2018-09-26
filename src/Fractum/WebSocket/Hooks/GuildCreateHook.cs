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
        public Task RunAsync(JToken data, IFractumCache cache)
        {
            var guild = data.ToObject<Guild>();

            cache.Guilds.AddOrUpdate(guild.Id, guild, (k, v) => guild ?? v);

            return Task.CompletedTask;
        }
    }
}
