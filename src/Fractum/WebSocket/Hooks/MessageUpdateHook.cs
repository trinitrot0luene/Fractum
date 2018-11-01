using System.Linq;
using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.Entities.WebSocket;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class MessageUpdateHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, GatewaySession session)
        {
            var newMessage = (MessageUpdateEventModel) args;

            if (cache.TryGetGuild(newMessage.ChannelId, out var guild, SearchType.Channel)
                && guild.TryGet(newMessage.ChannelId, out CircularBuffer<CachedMessage> messages))
            {
                var oldMessage = messages.FirstOrDefault(x => x.Id == newMessage.Id);

                if (oldMessage is null)
                    return Task.CompletedTask;

                var clonedMessage = oldMessage.Clone() as CachedMessage;

                oldMessage.Update(newMessage);

                cache.Client.InvokeMessageUpdated(clonedMessage, oldMessage);
            }
            else if (cache.DmChannels.FirstOrDefault(x => x.Id == newMessage.ChannelId) is CachedDMChannel dmChannel
                && dmChannel.MessageBuffer is CircularBuffer<CachedMessage> dmMessages)
            {
                var oldMessage = dmMessages.FirstOrDefault(x => x.Id == newMessage.Id);

                if (oldMessage is null)
                    return Task.CompletedTask;

                var clonedMessage = oldMessage.Clone() as CachedMessage;

                oldMessage.Update(newMessage);

                cache.Client.InvokeMessageUpdated(clonedMessage, oldMessage);
            }

            return Task.CompletedTask;
        }
    }
}