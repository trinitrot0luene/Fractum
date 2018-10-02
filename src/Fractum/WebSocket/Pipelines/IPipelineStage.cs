using Fractum.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fractum.WebSocket.Pipelines
{
    public interface IPipelineStage<TData>
    {
        FractumCache Cache { get; }

        ISession Session { get; }

        SocketWrapper Socket { get; }

        FractumSocketClient Client { get; }

        Task CompleteAsync(TData data);
    }
}
