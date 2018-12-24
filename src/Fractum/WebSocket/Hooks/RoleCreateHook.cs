using System.Threading.Tasks;
using Fractum;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class RoleCreateHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, GatewaySession session)
        {
            var eventArgs = (RoleCreateEventModel) args;

            if (cache.TryGetGuild(eventArgs.GuildId, out var guild))
            {
                guild.AddOrReplace(eventArgs.Role);

                cache.Client.InvokeRoleCreated(guild.Guild, eventArgs.Role);
            }

            return Task.CompletedTask;
        }
    }
}