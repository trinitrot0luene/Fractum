using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.WebSocket.Core;
using Newtonsoft.Json.Linq;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class ReadyHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(JToken args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            session.SessionId = args.Value<string>("session_id");

            client.InvokeLog(new LogMessage(nameof(ReadyHook), "Ready", LogSeverity.Info));

            client.InvokeReady();

            return Task.CompletedTask;
        }
    }
}