using Fractum.Entities;
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
    /// <summary>
    /// Operates on the Gateway WebSocket to handle all connection-related logic.
    /// </summary>
    internal sealed class ConnectionStage : IPipelineStage<Payload>
    {
        public ISession Session { get; private set; }

        public IFractumCache Cache { get; private set; }

        public FractumSocketClient Client { get; private set; }

        public SocketWrapper Socket { get; private set; }

        public Timer HeartbeatTimer { get; private set; }

        public ConnectionStage(FractumSocketClient client)
        {
            Cache = client.Cache;
            Session = client.Session;
            Socket = client.Socket;
            Client = client;

            Socket.ConnectionClosed += ReconnectAsync;
        }

        /// <summary>
        /// Run the stage to completion.
        /// </summary>
        /// <param name="payload">The payload the stage will receive as input.</param>
        /// <returns></returns>
        public Task CompleteAsync(Payload payload)
        {
            switch (payload.OpCode)
            {
                #region Hello
                case OpCode.Hello:
                    // Regardless of what happens we want to start heartbeating on HELLO.
                    var heartbeatInterval = payload.DataObject.Value<int>("heartbeat_interval");
                    HeartbeatTimer = new Timer((_) => Task.Run(() => HeartbeatAsync()), null, heartbeatInterval, heartbeatInterval);
                    // If we aren't resuming, re-identify.
                    if (!Session.Resuming)
                    {
                        Session.Invalidate();
                        return IdentifyAsync();
                    }
                    else
                        return ResumeAsync();
                #endregion
                #region Reconnect
                case OpCode.Reconnect:
                    return Socket.DisconnectAsync();
                #endregion
                #region Invalid Session
                case OpCode.InvalidSession:
                    var isValid = payload.Data.ToObject<bool>();
                    // d: false, invalidate session and re-identify.
                    if (!isValid)
                    {
                        Session.Invalidate();
                        return IdentifyAsync();
                    }
                    else
                        return ResumeAsync();
                #endregion
                #region Heartbeat / ACK
                case OpCode.Heartbeat:
                    return AcknowledgeHeartbeatAsync();
                case OpCode.HeartbeatACK:
                    Session.WaitingForACK = false;
                    return Task.CompletedTask;
                #endregion
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Handle socket closures and subsequent reconnections.
        /// </summary>
        /// <param name="status">Details of the closure.</param>
        /// <returns></returns>
        private async Task ReconnectAsync(WebSocketCloseStatus status)
        {
            var statusCode = (int)status;
            if (statusCode == 1000 || statusCode == 4006)
                Session.Invalidated = true; // When the connection is re-established don't try to resume, re-identify.

            if (Session.Resuming) return; // We are trying to resume already and these disconnections are just failed reconnects.
            int backoffPower = 1;
            int backoff = 2;

            Session.Resuming = true; // Lock other closed event handlers and op2 Handling
            do
            {
                var computedBackoff = (int)Math.Pow(backoff, backoffPower) * 1000; // Keep raising our backoff up to a maximum of 900 minutes.

                await Task.Delay(computedBackoff <= 900000 ? computedBackoff : 900000);

                await Socket.ConnectAsync(); // Try to reconnect

                backoffPower++;
                Session.ReconnectionAttempts++; // Increment reconnection attempts.
            }
            while (Socket.ListenerTask.IsCanceled && Session.ReconnectionAttempts <= 3); // No listener and we haven't tried 3 reconnections.

            // Connection resumed successfully, the close handler can now exit.
            if (!Socket.ListenerTask.IsCanceled)
                return;

            // Fetch connection info from the gateway with new Url: 
            var gatewayInfo = await Client.GetSocketUrlAsync();
            if (gatewayInfo.SessionStartLimit["remaining"] == 0)
                throw new GatewayException("No new sessions can be started.");
            Socket.UpdateUrl(new Uri(gatewayInfo.Url + Consts.GATEWAY_PARAMS));
            // Continually try to reconnect.
            while (Socket.ListenerTask.IsCanceled)
            {
                var computedBackoff = (int)Math.Pow(backoff, backoffPower) * 1000; // Keep raising our backoff up to a maximum of 900 minutes.

                await Task.Delay(computedBackoff <= 900000 ? computedBackoff : 900000);

                await Socket.ConnectAsync(); // Try to reconnect

                backoffPower++;

                Session.ReconnectionAttempts++; // Redundant but potentially useful for debugging purposes.
            }
        }

        /// <summary>
        /// Send a resume payload to the gateway.
        /// </summary>
        /// <returns></returns>
        private Task ResumeAsync()
        {
            var resume = new Payload
            {
                OpCode = OpCode.Resume,
                Data = JToken.Parse(new
                {
                    token = Client.Config.Token,
                    session_id = Session.SessionId,
                    seq = Session.Seq
                }.Serialize())
            }.Serialize();

            return Socket.SendMessageAsync(resume);
        }

        /// <summary>
        /// Send an identify payload to the gateway.
        /// </summary>
        /// <returns></returns>
        private Task IdentifyAsync()
        {
            // Should reset cache here.   
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

        /// <summary>
        /// Send a heartbeat to the gateway.
        /// </summary>
        /// <returns></returns>
        private Task HeartbeatAsync()
        {
            if (Session.WaitingForACK)
            {
                HeartbeatTimer.Dispose();
                return Socket.DisconnectAsync();
            }
            var heartbeat = new
            {
                op = OpCode.Heartbeat,
                d = Session.Seq
            }.Serialize();
            Session.WaitingForACK = true;

            return Socket.SendMessageAsync(heartbeat);
        }

        /// <summary>
        /// In the case that the client receives a heartbeat from the socket, send an ACK back.
        /// </summary>
        /// <returns></returns>
        private Task AcknowledgeHeartbeatAsync()
        {
            var heartbeatAck = new
            {
                op = OpCode.HeartbeatACK,
                d = default(string),
                t = default(string),
                s = default(string)
            }.Serialize();

            return Socket.SendMessageAsync(heartbeatAck);
        }
    }
}
