using System.Linq;
using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.Entities.WebSocket;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class MessageDeleteHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, ISocketCache<ISyncedGuild> cache, ISession session)
        {
            var eventModel = (MessageDeleteEventModel) args;

            if (cache.TryGetGuild(eventModel.ChannelId, out var guild, SearchType.Channel)
            && guild.TryGet(eventModel.ChannelId, out CircularBuffer<CachedMessage> messages))
            {
                var message = messages.FirstOrDefault(x => x.Id == eventModel.Id);

                if (message != null)
                {
                    cache.Client.InvokeMessageDeleted(new CachedEntity<CachedMessage>(message));
                    messages.Remove(message);
                }
            }

            return Task.CompletedTask;
        }
    }
}