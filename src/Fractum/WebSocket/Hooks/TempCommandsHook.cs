using Fractum.Entities;
using Fractum.WebSocket.Pipelines;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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

            if (msg.Content.StartsWith(">") && msg.Author.Id == 233648473390448641)
            {
                switch (msg.Content.Substring(1, msg.Content.Length - 1))
                {
                    case "presence_test":
                        await msg.Channel.CreateMessageAsync($"Your status is: {(msg.Author as GuildMember)?.Presence?.Status.ToString() ?? "Uncached"}");
                        return;
                }
            }
        }
    }
}
