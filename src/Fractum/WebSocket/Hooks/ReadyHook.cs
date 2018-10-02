using Fractum.Entities;
using Fractum.WebSocket.Pipelines;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fractum.WebSocket.Hooks
{
    public sealed class ReadyHook : IEventHook<JToken>
    {
        public Task RunAsync(JToken args, IFractumCache cache, ISession session, FractumSocketClient client)
        {
            session.SessionId = args.Value<string>("session_id");

            client.InvokeLog(new LogMessage(nameof(ReadyHook), "Ready", LogSeverity.Info));

            return Task.CompletedTask;
        }
    }
}
