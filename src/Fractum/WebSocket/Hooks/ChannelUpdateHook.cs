using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.Pipelines;
using Newtonsoft.Json.Linq;

namespace Fractum.WebSocket.Hooks
{
    public class ChannelUpdateHook : IEventHook<JToken>
    {
        public Task RunAsync(JToken args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            GuildChannel updatedChannel = null;
            switch ((ChannelType) args.Value<int>("type"))
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

            cache.UpdateGuildCache(updatedChannel.GuildId,
                gc => { gc.Channels.AddOrUpdate(updatedChannel.Id, updatedChannel, (k, v) => updatedChannel ?? v); });

            client.InvokeLog(new LogMessage(nameof(ChannelCreateHook),
                $"Channel {updatedChannel.Name} was updated", LogSeverity.Verbose));

            return Task.CompletedTask;
        }
    }
}