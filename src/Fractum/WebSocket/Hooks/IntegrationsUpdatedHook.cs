using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;
using Newtonsoft.Json.Linq;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class IntegrationsUpdatedHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var id = args.Cast<IntegrationsUpdatedEventModel>();

            client.InvokeIntegrationsUpdated(cache[id]?.Guild);

            return Task.CompletedTask;
        }
    }
}