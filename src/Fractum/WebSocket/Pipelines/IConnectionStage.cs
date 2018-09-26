using Fractum.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fractum.WebSocket.Pipelines
{
    /// <summary>
    /// A pipeline stage responsible for the handling and response to payload OpCodes.
    /// </summary>
    public interface IConnectionStage<TData> : IPipelineStage<TData>
    {
        SocketWrapper Socket { get; }

        IStateCache State { get; }
    }
}
