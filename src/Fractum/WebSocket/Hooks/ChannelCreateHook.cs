using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.Utilities;
using Fractum.WebSocket.Core;
using Newtonsoft.Json.Linq;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class ChannelCreateHook : IEventHook<JToken>
    {
        public Task RunAsync(JToken args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            GuildChannel createdChannel = null;
            switch ((ChannelType) args.Value<int>("type"))
            {
                case ChannelType.GuildCategory:
                    createdChannel = args.ToObject<Category>();
                    break;
                case ChannelType.GuildText:
                    createdChannel = args.ToObject<TextChannel>();
                    break;
                case ChannelType.GuildVoice:
                    createdChannel = args.ToObject<VoiceChannel>();
                    break;
            }

            createdChannel.WithClient(client);

            if (cache.HasGuild(createdChannel.GuildId))
                cache[createdChannel.GuildId].AddOrUpdate(createdChannel, old => old = createdChannel);

            client.InvokeLog(new LogMessage(nameof(ChannelCreateHook),
                $"Channel {createdChannel.Name} was created", LogSeverity.Verbose));

            client.InvokeChannelCreated(createdChannel);

            return Task.CompletedTask;
        }
    }
}