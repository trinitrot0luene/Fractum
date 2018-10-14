using System.Collections.Generic;

namespace Fractum.Contracts
{
    /// <summary>
    ///     A pipeline stage responsible for handling event data sent by payloads.
    /// </summary>
    public interface IEventStage<TData> : IPipelineStage<TData>
    {
        Dictionary<string, IEventHook<TData>> Hooks { get; }

        IEventStage<TData> RegisterHook(string eventName, IEventHook<TData> hook);
    }
}