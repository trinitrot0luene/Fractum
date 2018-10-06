using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.Utilities;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.Pipelines;
using Newtonsoft.Json.Linq;

namespace Fractum.WebSocket.Hooks
{
    public class ChannelDeleteHook : IEventHook<JToken>
    {
        public Task RunAsync(JToken args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            GuildChannel deletedChannel = null;
            switch ((ChannelType) args.Value<int>("type"))
            {
                case ChannelType.GuildCategory:
                    deletedChannel = args.ToObject<Category>();
                    break;
                case ChannelType.GuildText:
                    deletedChannel = args.ToObject<TextChannel>();
                    break;
                case ChannelType.GuildVoice:
                    deletedChannel = args.ToObject<VoiceChannel>();
                    break;
            }

            cache.UpdateGuildCache(deletedChannel.GuildId, gc => { gc.Channels.TryRemove(deletedChannel.Id, out _); });

            client.InvokeLog(new LogMessage(nameof(ChannelDeleteHook), $"Channel {deletedChannel.Name} was deleted",
                LogSeverity.Verbose));

            client.InvokeChannelDeleted(new CachedEntity<GuildChannel>(deletedChannel));

            return Task.CompletedTask;
        }
    }
}