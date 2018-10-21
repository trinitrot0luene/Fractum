using System.Collections.Generic;
using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities.WebSocket;
using Fractum.WebSocket.EventModels;
using Newtonsoft.Json.Linq;

namespace Fractum.WebSocket.Core
{
    public sealed class EventStage : IPipelineStage<IPayload<EventModelBase>>
    {
        private readonly Dictionary<string, List<IEventHook<EventModelBase>>> Hooks;

        public EventStage(FractumSocketClient client)
        {
            Cache = client.Cache;
            Session = client.Session;
            Socket = client.Socket;
            Client = client;

            Hooks = new Dictionary<string, List<IEventHook<EventModelBase>>>();
        }

        /// <summary>
        ///     Gets the <see cref="FractumCache"/> which stores guild state.
        /// </summary>
        public FractumCache Cache { get; }

        /// <summary>
        ///     Gets the <see cref="ISession"/> which caches session data for the gateway connection.
        /// </summary>
        public ISession Session { get; }

        /// <summary>
        ///     Gets the <see cref="SocketWrapper"/> maintaining the gateway connection.
        /// </summary>
        public SocketWrapper Socket { get; }

        /// <summary>
        ///     Gets the <see cref="FractumSocketClient"/> for the <see cref="EventStage"/> to raise events and populate entities.
        /// </summary>
        public FractumSocketClient Client { get; }

        /// <summary>
        ///     Asynchronously enter the <see cref="EventStage"/> and execute it.
        /// </summary>
        /// <param name="data">The data to operate on.</param>
        /// <returns></returns>
        public async Task CompleteAsync(IPayload<EventModelBase> payload)
        {
            if (Hooks.TryGetValue(payload.Type ?? string.Empty, out var hooks))
                foreach (var hook in hooks)
                    await hook.RunAsync(payload.Data, Cache, Session, Client);
        }

        /// <summary>
        ///     Register an <see cref="IEventHook{TArgs}"/> to be executed on a specific gateway dispatch.
        /// </summary>
        /// <param name="eventName">The target dispatch name.</param>
        /// <param name="hook">The hook to be executed.</param>
        /// <returns></returns>
        public EventStage RegisterHook(string eventName, IEventHook<EventModelBase> hook)
        {
            if (Hooks.TryGetValue(eventName.ToUpper(), out var existingHooks))
                existingHooks.Add(hook);
            else
                Hooks.Add(eventName.ToUpper(), new List<IEventHook<EventModelBase>> {hook});

            return this;
        }

        /// <summary>
        ///     Remove all hooks registered to an event.
        /// </summary>
        /// <param name="eventName">The target dispatch name.</param>
        public void ClearHooks(string eventName)
            => Hooks.Remove(eventName.ToUpperInvariant());
    }
}