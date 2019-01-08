using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Fractum;
using Fractum.WebSocket;
using Fractum.Extensions;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket
{
    /// <summary>
    ///     Operates on the gateway socket to handle all connection-related logic.
    /// </summary>
    internal sealed class ConnectionStage : IPipelineStage<IPayload<EventModelBase>>
    {
        /// <summary>
        ///     Run the stage to completion.
        /// </summary>
        /// <param name="payload">The payload the stage will receive as input.</param>
        /// <returns></returns>
        public Task CompleteAsync(IPayload<EventModelBase> payload, PipelineContext context)
        {
            if (context.Socket?.State != WebSocketState.Open)
                return Task.CompletedTask;

            context.Session.Seq = payload.Seq ?? context.Session.Seq;
            switch (payload.OpCode)
            {
                #region Hello

                case OpCode.Hello:

                    context.Client.InvokeLog(new LogMessage(nameof(ConnectionStage), "Hello", LogSeverity.Debug));

                    var heartbeatInterval = ((HelloEventModel) payload.Data).HeartbeatInterval;
                    context.Client.HeartbeatTimer = new Timer(_ => Task.Run(() => context.Client.HeartbeatAsync()), null, heartbeatInterval,
                        heartbeatInterval);
                    
                    if (context.Session.Invalidated || context.Session.SessionId == null)
                    {
                        context.Client.Session = new GatewaySession();
                        context.Client.Cache = new GatewayCache(context.Client);
                        return context.Client.IdentifyAsync();
                    }
                    else
                    {
                        return context.Client.ResumeAsync();
                    }

                #endregion

                #region Reconnect

                case OpCode.Reconnect:
                    context.Client.InvokeLog(new LogMessage(nameof(ConnectionStage), "Received reconnect request, reconnecting",
                        LogSeverity.Warning));
                    return context.Socket.DisconnectAsync(GatewayCloseCode.UnknownError);

                #endregion

                #region Invalid Session

                case OpCode.InvalidSession:
                    var isValid = ((InvalidSessionEventModel) payload.Data).Resumable;

                    context.Client.InvokeLog(new LogMessage(nameof(ConnectionStage), $"Invalid Session, Resumable: {isValid}", LogSeverity.Warning));
                    if (!isValid)
                    {
                        context.Session.Invalidated = true;

                        return Task.Delay(1000).ContinueWith(x => context.Client.IdentifyAsync());
                    }
                    else
                        return context.Client.ResumeAsync();

                #endregion

                #region Heartbeat / ACK

                case OpCode.Heartbeat:
                    context.Client.InvokeLog(new LogMessage(nameof(ConnectionStage), "Received heartbeat request from Gateway",
                        LogSeverity.Debug));
                    return context.Client.HeartbeatAsync();
                case OpCode.HeartbeatACK:
                    context.Session.WaitingForACK = false;
                    context.Client.Latency = (int) (DateTimeOffset.UtcNow - context.Client.LastSentHeartbeat).TotalMilliseconds;
                    context.Client.InvokeLog(new LogMessage(nameof(ConnectionStage), "Heartbeat ACK", LogSeverity.Debug));
                    return Task.CompletedTask;

                #endregion
            }

            return Task.CompletedTask;
        }
    }
}