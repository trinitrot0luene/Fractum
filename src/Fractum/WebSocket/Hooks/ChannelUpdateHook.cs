using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.Entities.WebSocket;
using Fractum.Utilities;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class ChannelUpdateHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var eventModel = (ChannelCreateUpdateOrDeleteEventModel) args;

            var oldChannel = cache[eventModel.GuildId].GetChannel(eventModel.Id);

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

            client.InvokeLog(new LogMessage(nameof(ChannelCreateHook),
                $"Channel {updatedChannel.Name} was updated", LogSeverity.Verbose));

            client.InvokeChannelUpdated(new CachedEntity<CachedGuildChannel>(oldChannel), updatedChannel);

            return Task.CompletedTask;
        }
    }
}