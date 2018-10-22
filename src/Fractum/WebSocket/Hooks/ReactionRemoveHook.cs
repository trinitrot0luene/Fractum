using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities.WebSocket;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class ReactionRemoveHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var eventModel = (ReactionRemoveEventModel) args;

            client.InvokeReactionRemoved(new CachedReaction(eventModel));

            return Task.CompletedTask;
        }
    }
}