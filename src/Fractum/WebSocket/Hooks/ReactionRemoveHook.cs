﻿using System.Threading.Tasks;
using Fractum;
using Fractum.WebSocket;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class ReactionRemoveHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, GatewayCache cache, GatewaySession session)
        {
            var eventModel = (ReactionRemoveEventModel) args;

            cache.Client.InvokeReactionRemoved(new CachedReaction(eventModel));

            return Task.CompletedTask;
        }
    }
}