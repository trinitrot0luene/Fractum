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
        private Dictionary<string, IEventHook<JToken>[]> Hooks;

        public IFractumCache Cache { get; set; }

        public EventStage(IFractumCache cache)
            => Cache = cache;

        public void RegisterHook(string eventName, IEventHook<JToken> hook)
        {
            if (Hooks.TryGetValue(eventName.ToUpper(), out var existingHooks))
            {
                Array.Resize(ref existingHooks, existingHooks.Length + 1);
                existingHooks[existingHooks.Length - 1] = hook;
            }
            else
                Hooks.Add(eventName.ToUpper(), new[] { hook });
        }

        public async Task CompleteAsync(Payload payload)
        {
            if (Hooks.TryGetValue(payload.Type, out var hooks))
                foreach (var hook in hooks)
                    await hook.RunAsync(payload.Data, Cache);
        }
    }
}
