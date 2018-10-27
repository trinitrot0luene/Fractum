using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.Entities.WebSocket;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class ChannelCreateHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, ISocketCache<ISyncedGuild> cache, ISession session)
        {
            var eventModel = (ChannelCreateUpdateOrDeleteEventModel) args;

            CachedGuildChannel createdChannel = null;
            switch (eventModel.Type)
            {
                case ChannelType.GuildCategory:
                    createdChannel = new CachedCategory(cache, eventModel);
                    break;
                case ChannelType.GuildText:
                    createdChannel = new CachedTextChannel(cache, eventModel);
                    break;
                case ChannelType.GuildVoice:
                    createdChannel = new CachedVoiceChannel(cache, eventModel);
                    break;
            }

            cache.Client.InvokeLog(new LogMessage(nameof(ChannelCreateHook),
                $"Channel {createdChannel.Name} was created", LogSeverity.Verbose));

            cache.Client.InvokeChannelCreated(createdChannel);

            return Task.CompletedTask;
        }
    }
}