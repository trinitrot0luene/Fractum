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
            cache.UpdateGuildCache(createdChannel.GuildId,
                gc => { gc.Channels.AddOrUpdate((a, b) => a.Id == b.Id, createdChannel, oldChannel => oldChannel = createdChannel ?? oldChannel); });

            client.InvokeLog(new LogMessage(nameof(ChannelCreateHook),
                $"Channel {createdChannel.Name} was created", LogSeverity.Verbose));

            client.InvokeChannelCreated(createdChannel);

            return Task.CompletedTask;
        }
    }
}