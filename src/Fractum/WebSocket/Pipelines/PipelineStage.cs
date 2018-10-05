using System.Threading.Tasks;
using Fractum.WebSocket.Entities;

namespace Fractum.WebSocket.Pipelines
{
    public abstract class PipelineStage : IPipelineStage<Payload>
    {
        public abstract FractumCache Cache { get; }

        public abstract ISession Session { get; }

        public abstract SocketWrapper Socket { get; }

        public abstract FractumSocketClient Client { get; }

        public abstract Task CompleteAsync(Payload payload);
    }
}