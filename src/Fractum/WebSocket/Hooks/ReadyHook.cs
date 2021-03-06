﻿using System.Threading.Tasks;
using Fractum;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class ReadyHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, GatewayCache cache, GatewaySession session)
        {
            var eventModel = (ReadyEventModel) args;

            session.SessionId = eventModel.SessionId;

            cache.Client.InvokeLog(new LogMessage(nameof(ReadyHook), "Ready", LogSeverity.Info));

            cache.Client.InvokeReady();

            return Task.CompletedTask;
        }
    }
}