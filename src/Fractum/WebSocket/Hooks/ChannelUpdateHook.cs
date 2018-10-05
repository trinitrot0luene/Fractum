using Fractum.Entities;
using Fractum.WebSocket.Pipelines;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fractum.WebSocket.Hooks
{
    public sealed class ChannelUpdateHook : IEventHook<JToken>
    {
        public Task RunAsync(JToken args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            GuildChannel updatedChannel = null;
            switch (args.Value<ChannelType>("type"))
            {
                case ChannelType.GuildCategory:
                    updatedChannel = args.ToObject<Category>();
                    break;
                case ChannelType.GuildText:
                    updatedChannel = args.ToObject<TextChannel>();
                    break;
                case ChannelType.GuildVoice:
                    updatedChannel = args.ToObject<VoiceChannel>();
                    break;
            }
            if (cache.Guilds.TryGetValue(updatedChannel.GuildId, out var guild))
                guild.Channels.AddOrUpdate(updatedChannel.Id, updatedChannel, (k, v) => v = updatedChannel ?? v);

            return Task.CompletedTask;
        }
    }
}
