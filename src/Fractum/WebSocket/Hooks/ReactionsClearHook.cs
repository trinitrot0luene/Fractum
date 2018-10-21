using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;
using Newtonsoft.Json.Linq;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class ReactionsClearHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            client.InvokeReactionsCleared(args.Value<ulong>("message_id"), args.Value<ulong>("channel_id"), args.Value<ulong?>("guild_id"));

            return Task.CompletedTask;
        }
    }
}