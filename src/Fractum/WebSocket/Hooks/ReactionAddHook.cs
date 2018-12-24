using System.Threading.Tasks;
using Fractum;
using Fractum.WebSocket;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class ReactionAddHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, GatewaySession session)
        {
            var eventModel = (ReactionAddEventModel) args;

            cache.Client.InvokeReactionAdded(new CachedReaction(eventModel));

            return Task.CompletedTask;
        }
    }
}