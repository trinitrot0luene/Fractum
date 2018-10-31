using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class EmojisUpdateHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, ISession session)
        {
            var eventArgs = (EmojisUpdateEventModel) args;

            if (cache.TryGetGuild(eventArgs.GuildId, out var guild))
            {
                guild.AddOrReplace(eventArgs.Emojis);

                cache.Client.InvokeLog(new LogMessage(nameof(EmojisUpdateHook),
                    $"Emojis updated for {guild?.Guild.Name ?? "Unknown Guild"}", LogSeverity.Debug));

                cache.Client.InvokeEmojisUpdated(guild?.Guild);
            }

            return Task.CompletedTask;
        }
    }
}