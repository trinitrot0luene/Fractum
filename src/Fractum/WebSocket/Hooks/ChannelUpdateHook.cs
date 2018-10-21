using System.Linq;
using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.Utilities;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;
using Newtonsoft.Json.Linq;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class ChannelUpdateHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, ISession session, FractumSocketClient client)
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

            var oldChannel = cache[updatedChannel.GuildId].GetChannels().FirstOrDefault(x => x.Id == updatedChannel.Id);

            cache[updatedChannel.GuildId].AddOrUpdate(updatedChannel, old => old = updatedChannel);

            client.InvokeLog(new LogMessage(nameof(ChannelCreateHook),
                $"Channel {updatedChannel.Name} was updated", LogSeverity.Verbose));

            client.InvokeChannelUpdated(new CachedEntity<GuildChannel>(oldChannel), updatedChannel);

            return Task.CompletedTask;
        }
    }
}