﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket
{
    public sealed class EventStage : IPipelineStage<IPayload<EventModelBase>>
    {
        private readonly Dictionary<string, List<IEventHook<EventModelBase>>> Hooks;

        public EventStage(FractumSocketClient client)
        {
            Hooks = new Dictionary<string, List<IEventHook<EventModelBase>>>();
        }

        /// <summary>
        ///     Asynchronously enter the <see cref="EventStage" /> and execute it.
        /// </summary>
        /// <param name="data">The data to operate on.</param>
        /// <param name="ctx">Contextual information when completing the stage.</param>
        /// <returns></returns>
        public async Task CompleteAsync(IPayload<EventModelBase> payload, PipelineContext ctx)
        {
            if (Hooks.TryGetValue(payload.Type ?? string.Empty, out var hooks))
                foreach (var hook in hooks)
                    await hook.RunAsync(payload.Data, ctx.Cache, ctx.Session);
        }

        /// <summary>
        ///     Register an <see cref="IEventHook{TArgs}" /> to be executed on a specific gateway dispatch.
        /// </summary>
        /// <param name="eventName">The target dispatch name.</param>
        /// <param name="hook">The hook to be executed.</param>
        /// <returns></returns>
        public EventStage RegisterHook(string eventName, IEventHook<EventModelBase> hook)
        {
            if (Hooks.TryGetValue(eventName.ToUpper(), out var existingHooks))
                existingHooks.Add(hook);
            else
                Hooks.Add(eventName.ToUpper(), new List<IEventHook<EventModelBase>> {hook});

            return this;
        }

        /// <summary>
        ///     Remove all hooks registered to an event.
        /// </summary>
        /// <param name="eventName">The target dispatch name.</param>
        public void ClearHooks(string eventName)
            => Hooks.Remove(eventName.ToUpperInvariant());
    }
}