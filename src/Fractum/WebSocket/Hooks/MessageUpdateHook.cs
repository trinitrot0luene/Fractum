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
    internal sealed class MessageUpdateHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var newMessage = args.Cast<MessageUpdateEventModel>();

            var oldMessage = (cache.Guilds.SelectMany(x => x.GetChannels())
                    .FirstOrDefault(c => c.Id == newMessage.ChannelId && ((c as IMessageChannel)?.Messages.Any() ?? false))
                as IMessageChannel)?.Messages.FirstOrDefault(m => m.Id == newMessage.Id);

            // TODO: Add the rest of the message properties, e.g. attachments

            if (oldMessage is null)
                return Task.CompletedTask;

            var clonedMessage = oldMessage.Clone() as Message;

            oldMessage.Update(newMessage);

            client.InvokeMessageUpdated(new CachedEntity<Message>(clonedMessage, client.GetMessage(oldMessage.Channel, oldMessage.Id).GetAsync), oldMessage);

            return Task.CompletedTask;
        }
    }
}