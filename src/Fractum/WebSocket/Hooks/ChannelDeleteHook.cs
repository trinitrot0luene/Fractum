using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.Utilities;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;
using Newtonsoft.Json.Linq;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class ChannelDeleteHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var eventArgs = args.Cast<ChannelDeleteEventModel>();

            GuildChannel deletedChannel = null;
            switch (eventArgs.)
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

            if (cache.HasGuild(deletedChannel.GuildId))
                cache[deletedChannel.GuildId].Remove(deletedChannel);

            client.InvokeLog(new LogMessage(nameof(ChannelDeleteHook), $"Channel {deletedChannel.Name} was deleted",
                LogSeverity.Verbose));

            client.InvokeChannelDeleted(new CachedEntity<GuildChannel>(deletedChannel));

            return Task.CompletedTask;
        }
    }
}