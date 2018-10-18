using System.Linq;
using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.Utilities;
using Fractum.WebSocket.Core;
using Newtonsoft.Json.Linq;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class MessageUpdateHook : IEventHook<JToken>
    {
        public Task RunAsync(JToken args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var newMessage = args.ToObject<Message>();

            var oldMessage = (cache.Guilds.SelectMany(x => x.GetChannels())
                    .FirstOrDefault(c => c.Id == newMessage.ChannelId && ((c as IMessageChannel)?.Messages.Any() ?? false))
                as IMessageChannel)?.Messages.FirstOrDefault(m => m.Id == newMessage.Id);

            // TODO: Add the rest of the message properties, e.g. attachments
            // TODO: Clone the message. (May need to rethink how cached properties are pulled in Message for that)

            if (oldMessage is null)
                return Task.CompletedTask;

            oldMessage.Update(newMessage);

            client.InvokeMessageUpdated(new CachedEntity<Message>(oldMessage), newMessage);

            cache[newMessage.GuildId ?? 0].AddOrCreate(newMessage);

            return Task.CompletedTask;
        }
    }
}