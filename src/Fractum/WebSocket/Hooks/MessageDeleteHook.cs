using System;
using System.Linq;
using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.Utilities;
using Fractum.WebSocket.Core;
using Newtonsoft.Json.Linq;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class MessageDeleteHook : IEventHook<JToken>
    {
        public Task RunAsync(JToken args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            GuildCache gc = null;
            var guildId = args.Value<ulong?>("guild_id");
            if (guildId.HasValue)
            {
                gc = cache[guildId.Value];
                var message = gc.GetMessages(args.Value<ulong>("channel_id"))
                    .FirstOrDefault(x => x.Id == args.Value<ulong>("id"));
                if (message != null)
                {
                    client.InvokeMessageDeleted(new CachedEntity<Message>(message));
                    gc.Remove(message);
                }
            }

            return Task.CompletedTask;
        }
    }
}