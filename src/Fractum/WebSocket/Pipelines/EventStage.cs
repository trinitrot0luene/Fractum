using Fractum.Entities;
using Fractum.WebSocket;
using Fractum.WebSocket.Entities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fractum.WebSocket.Pipelines
{
    public sealed class EventStage : IPipelineStage<Payload>
    {
        private Dictionary<string, List<IEventHook<JToken>>> Hooks;

        public FractumCache Cache { get; set; }

        public ISession Session { get; set; }

        public SocketWrapper Socket { get; set; }

        public FractumSocketClient Client { get; set; }

        public EventStage(FractumSocketClient client)
        {
            Cache = client.Cache;
            Session = client.Session;
            Socket = client.Socket;
            Client = client;

            Hooks = new Dictionary<string, List<IEventHook<JToken>>>();
        }

        public EventStage RegisterHook(string eventName, IEventHook<JToken> hook)
        {
            if (Hooks.TryGetValue(eventName.ToUpper(), out var existingHooks))
                existingHooks.Add(hook);
            else
                Hooks.Add(eventName.ToUpper(), new List<IEventHook<JToken>>() { hook });

            return this;
        }

        public async Task CompleteAsync(Payload payload)
        {
            if (Hooks.TryGetValue(payload.Type ?? string.Empty, out var hooks))
                foreach (var hook in hooks)
                    await hook.RunAsync(payload.Data, Cache, Session, Client);
        }
    }
}
