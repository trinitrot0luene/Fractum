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
                    .FirstOrDefault(c => c.Id == newMessage.ChannelId)
                as IMessageChannel)?.Messages.FirstOrDefault(m => m.Id == newMessage.Id);

            // oldMessage.Content = newMessage.Content ?? oldMessage.Content;
            // oldMessage.MentionedRoleIds = newMessage.MentionedRoleIds ?? oldMessage.MentionedRoleIds;
            // oldMessage.MentionedUsers = newMessage.MentionedUsers ?? oldMessage.MentionedUsers;
            // oldMessage.Embeds = newMessage.Embeds ?? oldMessage.Embeds;
            // TODO: Add the rest of the message properties, e.g. attachments

            cache.Populate(newMessage);

            client.InvokeMessageUpdated(new CachedEntity<Message>(oldMessage), newMessage);

            cache[newMessage.GuildId ?? 0].AddOrCreate(newMessage);

            return Task.CompletedTask;
        }
    }
}