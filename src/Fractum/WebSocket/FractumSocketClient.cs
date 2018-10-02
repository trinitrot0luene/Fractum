using Fractum.Entities;
using Fractum.WebSocket;
using Fractum.WebSocket.Hooks;
using Fractum.WebSocket.Pipelines;
using Fractum.Rest;
using Fractum.WebSocket.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fractum.WebSocket
{
    public sealed class FractumSocketClient
    {
        internal SocketWrapper Socket;
        internal IFractumCache Cache; // TODO: Implement
        internal ISession Session; // TODO: Implement
        internal FractumSocketConfig Config;

        private FractumRestClient _rest;

        private IPipeline<Payload> _pipeline;

        public FractumSocketClient(FractumSocketConfig config)
        {
            Config = config;
            Cache = new FractumCache(Config);
            Session = new Session();
            _rest = new FractumRestClient(Config);
        }

        public void UseDefaultPipeline()
        {
            var connectionStage = new ConnectionStage(this);

            var eventStage = new EventStage(this)
                .RegisterHook("GUILD_CREATE", new GuildCreateHook())
                .RegisterHook("READY", new ReadyHook());

            _pipeline = new PayloadPipeline()
                .AddStage(connectionStage)
                .AddStage(eventStage);
        }

        public async Task InitialiseAsync()
        {
            var gatewayInfo = await _rest.GetSocketUrlAsync();
            if (gatewayInfo.SessionStartLimit["remaining"] <= 0)
                throw new InvalidOperationException("No new sessions can be started.");

            Socket = new SocketWrapper(new Uri(gatewayInfo.Url + Consts.GATEWAY_PARAMS));

            if (_pipeline is null)
                UseDefaultPipeline();

            Socket.PayloadReceived += async (payload) =>
            {
                var logMessage = await _pipeline.CompleteAsync(payload);
                if (logMessage != null)
                    InvokeLog(logMessage);
            };
        }

        public Task<GatewayBotResponse> GetSocketUrlAsync()
            => _rest.GetSocketUrlAsync();

        public Task ConnectAsync()
            => Socket.ConnectAsync();

        internal void InvokeLog(LogMessage msg)
           => Log?.Invoke(msg);

        public event Func<LogMessage, Task> Log;
    }
}
