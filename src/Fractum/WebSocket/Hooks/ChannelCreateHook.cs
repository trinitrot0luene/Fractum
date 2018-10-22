using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.Entities.WebSocket;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class ChannelCreateHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, ISession session, FractumSocketClient client)
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

            client.InvokeLog(new LogMessage(nameof(ChannelCreateHook),
                $"Channel {createdChannel.Name} was created", LogSeverity.Verbose));

            client.InvokeChannelCreated(createdChannel);

            return Task.CompletedTask;
        }
    }
}