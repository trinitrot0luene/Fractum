using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class EmojisUpdateHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var eventArgs = (EmojisUpdateEventModel) args;

            var guild = cache[eventArgs.GuildId];

            guild.Replace(eventArgs.Emojis);

            client.InvokeLog(new LogMessage(nameof(EmojisUpdateHook),
                $"Emojis updated for {guild?.Guild.Name ?? "Unknown Guild"}", LogSeverity.Debug));

            client.InvokeEmojisUpdated(guild?.Guild);

            return Task.CompletedTask;
        }
    }
}