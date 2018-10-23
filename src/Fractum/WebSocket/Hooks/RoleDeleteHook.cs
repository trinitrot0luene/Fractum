using System.Linq;
using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class RoleDeleteHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var eventArgs = (RoleDeleteEventModel) args;

            var role = cache[eventArgs.GuildId].GetRoles().First(x => x.Id == eventArgs.RoleId);

            cache[eventArgs.GuildId].Remove(role);

            client.InvokeRoleDeleted(cache[eventArgs.GuildId].Guild, role);

            return Task.CompletedTask;
        }
    }
}