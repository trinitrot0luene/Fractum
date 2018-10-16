using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.WebSocket.Core;
using Newtonsoft.Json.Linq;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class IntegrationsUpdatedHook : IEventHook<JToken>
    {
        public Task RunAsync(JToken args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var id = args.ToObject<ulong>();

            client.InvokeIntegrationsUpdated(cache[id]?.Guild);

            return Task.CompletedTask;
        }
    }
}