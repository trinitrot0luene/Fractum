using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;
using Newtonsoft.Json.Linq;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class RoleCreateHook : IEventHook<JToken>
    {
        public Task RunAsync(JToken args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var eventArgs = args.ToObject<GuildRoleEventModel>();

            cache[eventArgs.GuildId].AddOrUpdate(eventArgs.Role, old => old = eventArgs.Role);

            client.InvokeRoleCreated(cache[eventArgs.GuildId].Guild, eventArgs.Role);

            return Task.CompletedTask;
        }
    }
}