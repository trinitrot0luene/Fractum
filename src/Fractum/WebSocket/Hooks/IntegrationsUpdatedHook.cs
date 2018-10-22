using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class IntegrationsUpdatedHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var eventModel = (IntegrationsUpdatedEventModel) args;

            client.InvokeIntegrationsUpdated(cache[eventModel.GuildId]?.Guild);

            return Task.CompletedTask;
        }
    }
}