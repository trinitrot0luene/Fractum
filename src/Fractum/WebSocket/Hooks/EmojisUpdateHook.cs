using System.Collections;
using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.Entities.WebSocket;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class EmojisUpdateHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, GatewaySession session)
        {
            var eventArgs = (EmojisUpdateEventModel) args;

            if (cache.TryGetGuild(eventArgs.GuildId, out var guild))
            {
                guild.AddOrReplace(eventArgs.Emojis);

                cache.Client.InvokeLog(new LogMessage(nameof(EmojisUpdateHook),
                    $"Emojis updated for {guild?.Guild.Name ?? "Unknown Guild"}", LogSeverity.Debug));

                cache.Client.InvokeEmojisUpdated(new Cacheable<CachedGuild>(guild?.Guild), eventArgs.Emojis);
            }

            return Task.CompletedTask;
        }
    }
}