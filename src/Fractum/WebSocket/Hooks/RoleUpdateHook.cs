using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class RoleUpdateHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, GatewaySession session)
        {
            var eventArgs = (RoleUpdateEventModel) args;

            if (cache.TryGetGuild(eventArgs.GuildId, out var guild))
            {
                guild.AddOrReplace(eventArgs.Role);

                cache.Client.InvokeRoleUpdated(guild.Guild, eventArgs.Role);
            }

            return Task.CompletedTask;
        }
    }
}