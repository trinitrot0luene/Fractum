using Fractum.Entities.Extensions;
using Fractum.WebSocket.Entities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fractum.WebSocket.Pipelines
{
    public sealed class ConnectionStage : IConnectionStage<Payload>
    {
        public IStateCache State { get; private set; }

        public IFractumCache Cache { get; private set; }

        public SocketWrapper Socket { get; private set; }

        public Timer HeartbeatTimer { get; private set; }

        public object FreezeObject { get; private set; }

        public ConnectionStage(IStateCache state, SocketWrapper socket)
        {
            State = state;
            Socket = socket;

            Socket.ConnectionClosed += HandleClosedAsync;
        }

        private async Task HandleClosedAsync(WebSocketCloseStatus status)
        {
            FreezeObject = new object();

            await Socket.ConnectAsync();
            switch((int)status)
            {
                case 1000:
                case 4006:
                    State.Reset();
                    FreezeObject = default;
                    await IdentifyAsync();
                    return;
                default:
                    if (State.ReconnectionAttempts <= 3)
                    {
                        State.ReconnectionAttempts++;
                        await ResumeAsync();
                    }
                    return;
            }
        }

        public Task ResumeAsync()
        {

            return Socket.SendMessageAsync(resume);
        }

        public Task CompleteAsync(Payload payload)
        {
            if (FreezeObject != null)
                return Task.CompletedTask;

            switch (payload.OpCode)
            {
                case OpCode.HeartbeatACK:
                    State.IsWaitingForACK = false;
                    break;
                case OpCode.Hello:
                    State.HeartbeatInterval = payload.DataObject.Value<int>("heartbeat_interval");
                    HeartbeatTimer = new Timer((_) => Task.Run(() => HeartbeatAsync()), null, State.HeartbeatInterval, State.HeartbeatInterval);

                    return IdentifyAsync();
                case OpCode.Reconnect:
                    
                    break;
            }

            return Task.CompletedTask;
        }

        public Task IdentifyAsync()
        {
            State = new StateCache();
            Cache = new FractumCache(Cache.Config);

            var identify = new Payload
            {
                OpCode = OpCode.Identify,
                Data = JToken.Parse(new
                {
                    token = Cache.Config.Token,
                    properties = new Dictionary<string, string>()
                    {
                        { "$os", Environment.OSVersion.ToString() },
                        { "$browser", Assembly.GetExecutingAssembly().FullName },
                        { "$device", Assembly.GetExecutingAssembly().FullName }
                    },
                    compress = false,
                    large_threshold = Cache.Config.LargeThreshold ?? 250
                }.Serialize())
            }.Serialize();

            return Socket.SendMessageAsync(identify);
        }

        public async Task ReconnectAsync()
        {
        }

        public Task HeartbeatAsync()
        {
            if (State.IsWaitingForACK)
            {
                HeartbeatTimer.Dispose();
                return ReconnectAsync();
            }
            var heartbeat = new
            {
                op = OpCode.Heartbeat,
                d = State.Seq
            }.Serialize();
            State.IsWaitingForACK = true;

            return Socket.SendMessageAsync(heartbeat);
        }
    }
}
