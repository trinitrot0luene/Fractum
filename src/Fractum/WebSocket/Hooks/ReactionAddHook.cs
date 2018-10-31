using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.Entities.WebSocket;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class ReactionAddHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, ISession session)
        {
            var eventModel = (ReactionAddEventModel) args;

            cache.Client.InvokeReactionAdded(new CachedReaction(eventModel));

            return Task.CompletedTask;
        }
    }
}