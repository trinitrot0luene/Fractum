using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class IntegrationsUpdatedHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, ISocketCache<ISyncedGuild> cache, ISession session)
        {
            var eventModel = (IntegrationsUpdatedEventModel) args;

            if (cache.TryGetGuild(eventModel.GuildId, out var guild))
                cache.Client.InvokeIntegrationsUpdated(guild.Guild);

            return Task.CompletedTask;
        }
    }
}