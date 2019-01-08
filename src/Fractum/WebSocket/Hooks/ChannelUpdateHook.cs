using System.Threading.Tasks;
using Fractum;
using Fractum.WebSocket;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class ChannelUpdateHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, GatewayCache cache, GatewaySession session)
        {
            var eventModel = (ChannelCreateUpdateOrDeleteEventModel) args;

            if (cache.TryGetGuild(eventModel.GuildId, out var guild) && guild.TryGet(eventModel.Id, out CachedGuildChannel oldChannel))
            {
                CachedGuildChannel updatedChannel = null;
                switch (eventModel.Type)
                {
                    case ChannelType.GuildCategory:
                        updatedChannel = new CachedCategory(cache, eventModel);
                        break;
                    case ChannelType.GuildText:
                        updatedChannel = new CachedTextChannel(cache, eventModel);
                        break;
                    case ChannelType.GuildVoice:
                        updatedChannel = new CachedVoiceChannel(cache, eventModel);
                        break;
                }

                cache.Client.InvokeLog(new LogMessage(nameof(ChannelCreateHook),
                    $"Channel {updatedChannel.Name} was updated", LogSeverity.Verbose));

                cache.Client.InvokeChannelUpdated(new Cacheable<CachedGuildChannel>(oldChannel), updatedChannel);
            }

            return Task.CompletedTask;
        }
    }
}