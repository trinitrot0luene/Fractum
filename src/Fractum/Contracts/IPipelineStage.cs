using System.Threading.Tasks;
using Fractum.WebSocket;
using Fractum.WebSocket.Core;

namespace Fractum.Contracts
{
    public interface IPipelineStage<TData>
    {
        /// <summary>
        ///     Gets the <see cref="FractumCache" /> which stores guild state.
        /// </summary>
        FractumCache Cache { get; }

        /// <summary>
        ///     Gets the <see cref="ISession" /> which caches session data for the gateway connection.
        /// </summary>
        ISession Session { get; }

        /// <summary>
        ///     Gets the <see cref="SocketWrapper" /> maintaining the gateway connection.
        /// </summary>
        SocketWrapper Socket { get; }

        /// <summary>
        ///     Gets the <see cref="FractumSocketClient" /> for the <see cref="IPipelineStage{TData}" /> to raise events and
        ///     populate entities.
        /// </summary>
        FractumSocketClient Client { get; }

        /// <summary>
        ///     Asynchronously enter the <see cref="IPipelineStage{TData}" /> and execute it.
        /// </summary>
        /// <param name="data">The data to operate on.</param>
        /// <returns></returns>
        Task CompleteAsync(TData data);
    }
}