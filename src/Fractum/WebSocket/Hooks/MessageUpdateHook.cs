using System.Linq;
using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.Entities.WebSocket;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class MessageUpdateHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, ISocketCache<ISyncedGuild> cache, ISession session)
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

                cache.Client.InvokeMessageUpdated(
                new CachedEntity<IMessage>(clonedMessage,
                    cache.Client.GetMessage(oldMessage.Channel, oldMessage.Id).GetAsync), oldMessage);
            }

            return Task.CompletedTask;
        }
    }
}