using Fractum.Entities;
using Fractum.WebSocket.Entities;
using Fractum.WebSocket.Pipelines;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fractum.WebSocket.Hooks
{
    public sealed class TempCommandsHook : IEventHook<JToken>
    {
        public async Task RunAsync(JToken args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var msg = args.ToObject<Message>();
            cache.AddAndPopulateMessage(msg);

            if (msg.Content.StartsWith(">") && !msg.Author.IsBot)
            {
                switch (msg.Content.Substring(1, msg.Content.Length - 1).ToLowerInvariant())
                {
                    case "update_test":
                        await client.UpdatePresenceAsync("With a shit c# lib", ActivityType.Playing);
                            return;
                    case "chunk_test":
                        await client.RequestMembersAsync(msg.Guild.Id);
                        return;
                    case "count":
                        await msg.Channel.CreateMessageAsync(msg.Guild.Members.Count.ToString());
                        return;
                }
            }
        }
    }
}
