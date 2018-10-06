using System.Threading.Tasks;
using Fractum.WebSocket.Core;

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