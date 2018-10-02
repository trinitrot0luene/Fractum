using Fractum.WebSocket.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fractum.WebSocket.Pipelines
{
    public abstract class PipelineStage : IPipelineStage<Payload>
    {
        public abstract IFractumCache Cache { get; }

        public abstract ISession Session { get; }

        public abstract SocketWrapper Socket { get; }

        public abstract FractumSocketClient Client { get; }

        public abstract Task CompleteAsync(Payload payload);
    }
}
