using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.Entities.WebSocket;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket
{
    public abstract class PipelineStage : IPipelineStage<IPayload<EventModelBase>>
    {
        internal abstract SocketWrapper Socket { get; }

        public abstract FractumCache Cache { get; }

        public abstract ISession Session { get; }

        public abstract FractumSocketClient Client { get; }

        public abstract Task CompleteAsync(IPayload<EventModelBase> payload, PipelineContext context);
    }
}