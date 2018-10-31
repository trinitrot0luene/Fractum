using System.Linq;
using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class RoleDeleteHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, ISession session)
        {
            var eventArgs = (RoleDeleteEventModel) args;

            if (cache.TryGetGuild(eventArgs.GuildId, out var guild))
            {
                if (guild.TryGet(eventArgs.RoleId, out Role role))
                    guild.RemoveRole(role.Id);

                cache.Client.InvokeRoleDeleted(guild.Guild, role);
            }

            return Task.CompletedTask;
        }
    }
}