using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.Pipelines;
using Newtonsoft.Json.Linq;

namespace Fractum.WebSocket.Hooks
{
    public sealed class ReadyHook : IEventHook<JToken>
    {
        public Task RunAsync(JToken args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            session.SessionId = args.Value<string>("session_id");

            client.InvokeLog(new LogMessage(nameof(ReadyHook), "Ready", LogSeverity.Info));

            return Task.CompletedTask;
        }
    }
}