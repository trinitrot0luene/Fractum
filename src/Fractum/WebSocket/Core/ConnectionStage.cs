using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.Entities.Extensions;
using Fractum.Entities.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fractum.WebSocket.Core
{
    /// <summary>
    ///     Operates on the gateway socket to handle all connection-related logic.
    /// </summary>
    internal sealed class ConnectionStage : IPipelineStage<Payload>
    {
        public ConnectionStage(FractumSocketClient client)
        {
            Cache = client.Cache;
            Session = client.Session;
            Socket = client.Socket;
            Client = client;

            Socket.ConnectionClosed += ReconnectAsync;
        }

        public Timer HeartbeatTimer { get; private set; }

        public ISession Session { get; }

        public FractumCache Cache { get; private set; }

        public FractumSocketClient Client { get; }

        public SocketWrapper Socket { get; }

        /// <summary>
        ///     Run the stage to completion.
        /// </summary>
        /// <param name="payload">The payload the stage will receive as input.</param>
        /// <returns></returns>
        public Task CompleteAsync(Payload payload)
        {
            Session.Seq = payload.Seq ?? Session.Seq;
            switch (payload.OpCode)
            {
                #region Hello

                case OpCode.Hello:
                    // Regardless of what happens we want to start heartbeating on HELLO.
                    var heartbeatInterval = payload.Data.Value<int>("heartbeat_interval");
                    HeartbeatTimer = new Timer(_ => Task.Run(() => HeartbeatAsync()), null, heartbeatInterval,
                        heartbeatInterval);
                    // If we aren't resuming, re-identify.
                    if (!Session.Resuming)
                    {
                        Session.Invalidate();
                        Cache = new FractumCache(Client);
                        return IdentifyAsync();
                    }
                    else
                    {
                        return ResumeAsync();
                    }

                #endregion

                #region Reconnect

                case OpCode.Reconnect:
                    Client.InvokeLog(new LogMessage(nameof(ConnectionStage), "Received reconnect request, reconnecting",
                        LogSeverity.Warning));
                    return Socket.DisconnectAsync(GatewayCloseCode.UnknownError);

                #endregion

                #region Invalid Session

                case OpCode.InvalidSession:
                    var isValid = payload.Data.ToObject<bool>();
                    // d: false, invalidate session and re-identify.
                    if (!isValid)
                    {
                        Client.InvokeLog(new LogMessage(nameof(ConnectionStage), "Invalid session, re-identifying.",
                            LogSeverity.Warning));
                        Session.Invalidate();
                        return IdentifyAsync();
                    }
                    else
                    {
                        return ResumeAsync();
                    }

                #endregion

                #region Heartbeat / ACK

                case OpCode.Heartbeat:
                    Client.InvokeLog(new LogMessage(nameof(ConnectionStage), "Received heartbeat request from Gateway",
                        LogSeverity.Debug));
                    return HeartbeatAsync();
                case OpCode.HeartbeatACK:
                    Session.WaitingForACK = false;
                    Client.Latency = (int) (DateTimeOffset.UtcNow - Client.LastSentHeartbeat).TotalMilliseconds;
                    Client.InvokeLog(new LogMessage(nameof(ConnectionStage), "Heartbeat ACK", LogSeverity.Debug));
                    return Task.CompletedTask;

                #endregion
            }

            return Task.CompletedTask;
        }

        /// <summary>
        ///     Handle socket closures and subsequent reconnections.
        /// </summary>
        /// <param name="status">Details of the closure.</param>
        /// <returns></returns>
        private async Task ReconnectAsync(WebSocketCloseStatus status)
        {
            Session.WaitingForACK = false; // We've been disconnected, reset checks for zombied connections.

            var statusCode = (int) status;

            Client.InvokeLog(new LogMessage(nameof(ConnectionStage),
                $"Disconnected from the gateway with error {statusCode}:{GatewayCloseCode.GetCodeName(statusCode)}.",
                LogSeverity.Error));

            if (statusCode == 1000 || statusCode == 4001 || statusCode == 4006)
                Session.Invalidated = true; // When the connection is re-established don't try to resume, re-identify.

            if (Session.Resuming)
                return; // We are trying to resume already and these disconnections are just failed reconnects.
            var backoffPower = 1;
            var backoff = 2;

            Client.InvokeLog(new LogMessage(nameof(ConnectionStage), "Attempting to reconnect...",
                LogSeverity.Warning));

            Session.Resuming = true; // Lock other closed event handlers and op2 Handling
            do
            {
                if (Session.ReconnectionAttempts != 0 && Session.ReconnectionAttempts % 4 == 0)
                {
                    // Fetch connection info from the gateway with new Url: 
                    var gatewayInfo = await Client.RestClient.GetSocketUrlAsync();
                    if (gatewayInfo?.SessionStartLimit["remaining"] == 0)
                        throw new GatewayException("No new sessions can be started");
                    if (gatewayInfo != null)
                        Session.GatewayUrl = gatewayInfo.Url;
                    Socket.UpdateUrl(new Uri(Session.GatewayUrl + Consts.GATEWAY_PARAMS));
                }

                var computedBackoff =
                    (int) Math.Pow(backoff, backoffPower) *
                    1000; // Keep raising our backoff up to a maximum of 900 seconds.

                await Task.Delay(computedBackoff <= 900000 ? computedBackoff : 900000);

                Client.InvokeLog(new LogMessage(nameof(ConnectionStage), $"Reconnection attempt {backoffPower}",
                    LogSeverity.Warning));

                await Socket.ConnectAsync(); // Try to reconnect

                backoffPower++;
                Session.ReconnectionAttempts++; // Increment reconnection attempts.
            } while (Socket.State != WebSocketState.Open); // No listener and we haven't tried 3 reconnections.

            // Connection resumed successfully, the close handler can now exit.
            Client.InvokeLog(new LogMessage(nameof(ConnectionStage), "Reconnected Successfully", LogSeverity.Warning));
        }

        /// <summary>
        ///     Send a resume payload to the gateway.
        /// </summary>
        /// <returns></returns>
        private Task ResumeAsync()
        {
            var resume = new SendPayload
            {
                op = OpCode.Resume,
                d = new
                {
                    token = Client.RestClient.Config.Token,
                    session_id = Session.SessionId,
                    seq = Session.Seq
                }
            }.Serialize();

            Client.InvokeLog(new LogMessage(nameof(ConnectionStage),
                $"Attempting to resume with seq at {Session.Seq} and session_id {Session.SessionId}",
                LogSeverity.Verbose));

            Session.Resuming = false;

            return Socket.SendMessageAsync(resume);
        }

        /// <summary>
        ///     Reset cache and send an identify payload to the gateway.
        /// </summary>
        /// <returns></returns>
        private Task IdentifyAsync()
        {
            Cache = new FractumCache(Client);
            var identify = new SendPayload
            {
                op = OpCode.Identify,
                d = new
                {
                    token = Cache.Client.RestClient.Config.Token,
                    properties = new Dictionary<string, string>
                    {
                        {"$os", Environment.OSVersion.ToString()},
                        {"$browser", Assembly.GetExecutingAssembly().FullName},
                        {"$device", Assembly.GetExecutingAssembly().FullName}
                    },
                    compress = false,
                    large_threshold = Cache.Client.RestClient.Config.LargeThreshold
                }
            }.Serialize();

            Session.Duration = DateTimeOffset.UtcNow;

            Client.InvokeLog(new LogMessage(nameof(ConnectionStage), "Identifying", LogSeverity.Verbose));

            return Socket.SendMessageAsync(identify);
        }

        /// <summary>
        ///     Send a heartbeat to the gateway.
        /// </summary>
        /// <returns></returns>
        private Task HeartbeatAsync()
        {
            if (Session.WaitingForACK)
            {
                HeartbeatTimer.Dispose();
                return Socket.DisconnectAsync();
            }

            var heartbeat = new SendPayload
            {
                op = OpCode.Heartbeat,
                d = Session.Seq
            }.Serialize();
            Session.WaitingForACK = true;
            Client.LastSentHeartbeat = DateTimeOffset.UtcNow;

            Client.InvokeLog(new LogMessage(nameof(ConnectionStage), "Heartbeat", LogSeverity.Debug));

            return Socket.SendMessageAsync(heartbeat);
        }
    }
}