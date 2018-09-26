using System;
using System.Collections.Generic;
using System.Text;

namespace Fractum.WebSocket.Pipelines
{
    /// <summary>
    /// A pipeline stage responsible for handling event data sent by payloads.
    /// </summary>
    public interface IEventStage<TData> : IPipelineStage<TData>
    {
        IFractumCache Cache { get; }

        Dictionary<string, IEventHook<TData>> Hooks { get; }

        IEventStage<TData> RegisterHook(string eventName, IEventHook<TData> hook);
    }
}
