﻿using System.Threading.Tasks;
using Fractum;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class GuildDeleteHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, GatewayCache cache, GatewaySession session)
        {
            var model = (GuildDeleteEventModel) args;

            if (cache.TryGetGuild(model.Id, out var guild))
            {
                cache.Client.InvokeLog(new LogMessage(nameof(GuildDeleteHook), $"Guild unavailable: {guild?.Guild.Name}",
                    LogSeverity.Info));

                cache.Client.InvokeGuildUnavailable(guild.Guild);

                cache.RemoveGuild(model.Id);
            }

            return Task.CompletedTask;
        }
    }
}