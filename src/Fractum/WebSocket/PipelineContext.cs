using System;
using Microsoft.Extensions.DependencyInjection;

namespace Fractum.WebSocket
{
    /// <summary>
    ///     Provides contextual data to a <see cref="IPipelineStage{TData}"/> during execution.
    /// </summary>
    public class PipelineContext
    {
        internal PipelineContext(FractumSocketClient client)
        {
            Client = client;
            Cache = client.Cache;
            Session = client.Session;
            Socket = client.Socket;
            Services = client.PipelineServices.BuildServiceProvider();
        }

        /// <summary>
        ///     Gets the <see cref="ISocketCache{T}" /> which stores guild state.
        /// </summary>
        public ISocketCache<ISyncedGuild> Cache { get; }

        /// <summary>
        ///     Gets the <see cref="FractumSocketClient"/> containing gateway state and caching information.
        /// </summary>
        public FractumSocketClient Client { get; }

        /// <summary>
        ///     Gets the <see cref="ISession" /> which caches session data for the gateway connection.
        /// </summary>
        public ISession Session { get; }

        /// <summary>
        ///     Gets the <see cref="SocketWrapper" /> maintaining the gateway connection.
        /// </summary>
        public SocketWrapper Socket { get; }

        /// <summary>
        ///     Gets the <see cref="IServiceProvider"/> containing services previously supplied to the <see cref="FractumSocketClient"/>.
        /// </summary>
        public IServiceProvider Services { get; }

        /// <summary>
        ///     Retrieves a required service from the <see cref="IServiceProvider"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetService<T>() => Services.GetRequiredService<T>();
    }
}