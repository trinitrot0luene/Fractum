using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;
using Newtonsoft.Json.Linq;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class RoleUpdateHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var eventArgs = args.Cast<RoleUpdateEventModel>();

            cache[eventArgs.GuildId].AddOrUpdate(eventArgs.Role, old => old = eventArgs.Role);

            client.InvokeRoleUpdated(cache[eventArgs.GuildId].Guild, eventArgs.Role);

            return Task.CompletedTask;
        }
    }
}