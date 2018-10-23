using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class ReactionsClearHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var eventModel = (ReactionsClearEventModel) args;

            client.InvokeReactionsCleared(eventModel.MessageId, eventModel.ChannelId, eventModel.GuildId);

            return Task.CompletedTask;
        }
    }
}