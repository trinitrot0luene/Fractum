using System.Linq;
using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;
using Newtonsoft.Json.Linq;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class RoleDeleteHook : IEventHook<JToken>
    {
        public Task RunAsync(JToken args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var eventArgs = args.ToObject<GuildRoleEventModel>();

            var role = cache[eventArgs.GuildId].GetRoles().First(x => x.Id == eventArgs.RoleId.Value);

            cache[eventArgs.GuildId].Remove(role);

            client.InvokeRoleDeleted(cache[eventArgs.GuildId].Guild, role);

            return Task.CompletedTask;
        }
    }
}