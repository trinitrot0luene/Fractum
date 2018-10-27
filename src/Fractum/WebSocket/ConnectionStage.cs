using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.Entities.WebSocket;
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
            if (context.Socket?.State != WebSocketState.Open) // If the socket isn't open, we don't want to run the event handler.
                return Task.CompletedTask;

            context.Session.Seq = payload.Seq ?? context.Session.Seq;
            switch (payload.OpCode)
            {
                #region Hello

                case OpCode.Hello:

                    context.Client.InvokeLog(new LogMessage(nameof(ConnectionStage), "Hello", LogSeverity.Debug));

                    // Regardless of what happens we want to start heartbeating on HELLO.
                    var heartbeatInterval = ((HelloEventModel) payload.Data).HeartbeatInterval;
                    context.Client.HeartbeatTimer = new Timer(_ => Task.Run(() => context.Client.HeartbeatAsync()), null, heartbeatInterval,
                        heartbeatInterval);
                    // If we aren't resuming, re-identify.
                    if (context.Session.Invalidated || context.Session.SessionId == null)
                    {
                        context.Client.Session = new Session();
                        context.Client.Cache = new FractumCache(context.Client);
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
                    // d: false, invalidate session and re-identify.
                    if (!isValid)
                    {
                        context.Session.Invalidated = true;



                        return Task.Delay(1000).ContinueWith(x => context.Client.IdentifyAsync());
                    }
                    else
                    {
                        return context.Client.ResumeAsync();
                    }

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