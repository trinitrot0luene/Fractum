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
        internal SocketWrapper _socket;

        internal IFractumCache cache; // TODO: Implement
        internal IStateCache state; // TODO: Implement
        internal Task SocketListenerTask;

        private FractumRestClient _rest;
        private FractumSocketConfig _config;

        private IPipeline<Payload> _pipeline;

        public FractumSocketClient(FractumSocketConfig config)
        {
            _config = config;

            _rest = new FractumRestClient(_config);
        }

        public void UseDefaultPipeline()
        {
            var connectionStage = new ConnectionStage(state, _socket);
            var eventStage = new EventStage(cache, state);
            eventStage.RegisterHook("GUILD_CREATE", new GuildCreateHook());

            _pipeline = new PayloadPipeline(cache, state, _socket);
            _pipeline.AddStage(eventStage);
            // TODO: Add Stages
        }

        public async Task InitialiseAsync()
        {
            var gatewayInfo = await _rest.GetSocketUrlAsync();
            if (gatewayInfo.SessionStartLimit["remaining"] <= 0)
                throw new InvalidOperationException("No new sessions can be started.");

            _socket = new SocketWrapper(new Uri(gatewayInfo.Url + Consts.GATEWAY_PARAMS));

            _socket.PayloadReceived += RunPipelineAsync;
        }

        private Task RunPipelineAsync(Payload payload)
            => _pipeline.CompleteAsync(payload);

        public Task ConnectAsync()
            => _socket.ConnectAsync();
    }
}
