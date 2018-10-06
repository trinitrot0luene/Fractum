using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.Pipelines;
using Newtonsoft.Json.Linq;

namespace Fractum.WebSocket.Hooks
{
    public sealed class ChannelCreateHook : IEventHook<JToken>
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

            cache.UpdateGuildCache(createdChannel.GuildId,
                gc => { gc.Channels.AddOrUpdate(createdChannel.Id, createdChannel, (k, v) => createdChannel ?? v); });

            client.InvokeLog(new LogMessage(nameof(ChannelCreateHook),
                $"Channel {createdChannel.Name} was created", LogSeverity.Verbose));

            return Task.CompletedTask;
        }
    }
}