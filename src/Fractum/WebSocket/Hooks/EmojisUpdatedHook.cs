using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;
using Newtonsoft.Json.Linq;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class EmojisUpdatedHook : IEventHook<JToken>
    {
        public Task RunAsync(JToken args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var eventArgs = args.ToObject<EmojiUpdatedEventModel>();

            var guild = cache[eventArgs.GuildId];

            guild.Replace(eventArgs.Emojis);

            client.InvokeLog(new LogMessage(nameof(EmojisUpdatedHook),
                $"Emojis updated for {guild?.Guild.Name ?? "Unknown Guild"}", LogSeverity.Debug));

            client.InvokeEmojisUpdated(guild?.Guild);

            return Task.CompletedTask;
        }
    }
}