using System.Collections.Generic;
using Fractum.WebSocket.EventModels;

namespace Fractum.Contracts
{
    /// <summary>
    ///     A pipeline stage responsible for handling event data sent by payloads.
    /// </summary>
    public interface IEventStage<TData> : IPipelineStage<TData>
    {
        Dictionary<string, IEventHook<EventModelBase>> Hooks { get; }

        IEventStage<TData> RegisterHook(string eventName, IEventHook<EventModelBase> hook);
    }
}