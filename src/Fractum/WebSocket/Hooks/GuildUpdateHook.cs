﻿using System.Threading.Tasks;
using Fractum;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class GuildUpdateHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, GatewayCache cache, GatewaySession session)
        {
            var eventModel = (GuildUpdateEventModel) args;

            if (cache.TryGetGuild(eventModel.Id, out var guildCache))
            {
                if (eventModel.IsUnavailable)
                {
                    guildCache.IsUnavailable = eventModel.IsUnavailable;
                    cache.Client.InvokeGuildUnavailable(guildCache.Guild);
                }

                guildCache.Update(eventModel);

                cache.Client.InvokeLog(new LogMessage(nameof(GuildUpdateHook), $"Guild: {eventModel.Name} was updated",
                    LogSeverity.Debug));

                cache.Client.InvokeGuildUpdated(guildCache.Guild);
            }

            return Task.CompletedTask;
        }
    }
}