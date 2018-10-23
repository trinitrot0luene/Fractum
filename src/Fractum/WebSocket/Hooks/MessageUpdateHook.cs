using System.Linq;
using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities.WebSocket;
using Fractum.Utilities;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class MessageUpdateHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var newMessage = (MessageUpdateEventModel) args;

            var oldMessage = cache.Guilds.FirstOrDefault(x => x.GetChannel(newMessage.ChannelId) != null)
                ?.GetMessages(newMessage.ChannelId)
                .FirstOrDefault(m => m.Id == newMessage.Id);

            if (oldMessage is null)
                return Task.CompletedTask;

            var clonedMessage = oldMessage.Clone() as CachedMessage;

            oldMessage.Update(newMessage);

            client.InvokeMessageUpdated(
                new CachedEntity<IMessage>(clonedMessage,
                    client.GetMessage(oldMessage.Channel, oldMessage.Id).GetAsync), oldMessage);

            return Task.CompletedTask;
        }
    }
}