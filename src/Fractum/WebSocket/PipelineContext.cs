using System;
using Microsoft.Extensions.DependencyInjection;

namespace Fractum.WebSocket
{
    /// <summary>
    ///     Provides contextual data to a <see cref="IPipelineStage{TData}"/> during execution.
    /// </summary>
    public class PipelineContext
    {
        internal PipelineContext(GatewayClient client)
        {
            Client = client;
            Cache = client.Cache;
            Session = client.Session;
            Socket = client.Socket;
            Services = client.PipelineServices.BuildServiceProvider();
        }

        /// <summary>
        ///     Gets the <see cref="SocketWrapper" /> maintaining the gateway connection.
        /// </summary>
        internal SocketWrapper Socket { get; }

        /// <summary>
        ///     Gets the <see cref="ISocketCache{T}" /> which stores guild state.
        /// </summary>
        public GatewayCache Cache { get; }

        /// <summary>
        ///     Gets the <see cref="GatewayClient"/> containing gateway state and caching information.
        /// </summary>
        public GatewayClient Client { get; }

        /// <summary>
        ///     Gets the <see cref="Session" /> which caches session data for the gateway connection.
        /// </summary>
        public GatewaySession Session { get; }

        /// <summary>
        ///     Gets the <see cref="IServiceProvider"/> containing services previously supplied to the <see cref="GatewayClient"/>.
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