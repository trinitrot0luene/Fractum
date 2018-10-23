using System.Linq;
using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities.WebSocket;
using Fractum.Utilities;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class MessageDeleteHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var eventModel = (MessageDeleteEventModel) args;

            if (eventModel.GuildId.HasValue && cache.HasGuild(eventModel.GuildId.Value))
            {
                var guild = cache[eventModel.GuildId.Value];

                var message = guild.GetMessages(eventModel.ChannelId)
                    .FirstOrDefault(m => m.Id == eventModel.Id);

                if (message != null)
                {
                    client.InvokeMessageDeleted(new CachedEntity<CachedMessage>(message));
                    guild.Remove(message);
                }
            }

            return Task.CompletedTask;
        }
    }
}