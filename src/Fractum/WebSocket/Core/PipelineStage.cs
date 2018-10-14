using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities.WebSocket;

namespace Fractum.WebSocket.Core
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