using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.WebSocket.Pipelines;
using Newtonsoft.Json.Linq;

namespace Fractum.WebSocket.Hooks
{
    public class GuildUpdateHook : IEventHook<JToken>
    {
        public Task RunAsync(JToken args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var guild = args.ToObject<Guild>();

            return Task.CompletedTask;
        }
    }
}