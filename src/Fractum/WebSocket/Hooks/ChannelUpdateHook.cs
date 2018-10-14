using System.Linq;
using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.Utilities;
using Fractum.WebSocket.Core;
using Newtonsoft.Json.Linq;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class ChannelUpdateHook : IEventHook<JToken>
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

            updatedChannel.WithClient(client);

            var oldChannel = cache.Guilds[updatedChannel.GuildId].Channels.FirstOrDefault(c => c.Id == updatedChannel.Id);

            cache.UpdateGuildCache(updatedChannel.GuildId,
                gc => { gc.Channels.AddOrUpdate((a, b) => a.Id == b.Id, updatedChannel, old => old = updatedChannel ?? old); });

            client.InvokeLog(new LogMessage(nameof(ChannelCreateHook),
                $"Channel {updatedChannel.Name} was updated", LogSeverity.Verbose));

            client.InvokeChannelUpdated(new CachedEntity<GuildChannel>(oldChannel), updatedChannel);

            return Task.CompletedTask;
        }
    }
}