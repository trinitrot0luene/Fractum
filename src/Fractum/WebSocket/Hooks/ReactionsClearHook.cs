using System.Threading.Tasks;
using Fractum;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class ReactionsClearHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, GatewaySession session)
        {
            var eventModel = (ReactionsClearEventModel) args;

            cache.Client.InvokeReactionsCleared(eventModel.MessageId, eventModel.ChannelId, eventModel.GuildId);

            return Task.CompletedTask;
        }
    }
}