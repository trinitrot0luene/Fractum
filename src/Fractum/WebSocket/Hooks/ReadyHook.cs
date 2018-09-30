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
        public Task RunAsync(JToken args, IFractumCache cache, IStateCache state)
        {
            state.SessionId = args.Value<string>("session_id");

            return Task.CompletedTask;
        }
    }
}
