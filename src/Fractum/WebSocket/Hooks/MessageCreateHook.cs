using System.Linq;
using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;
using Newtonsoft.Json.Linq;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class MessageCreateHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var message = args.Cast<MessageCreateEventModel>();

            if (message.GuildId.HasValue && cache.HasGuild(message.GuildId.Value))
            {
                var guild = cache[message.GuildId.Value];
                guild.AddOrCreate(message);
            }
            else return Task.CompletedTask;

            client.InvokeLog(new LogMessage(nameof(MessageCreateHook),
                $"Received message from {(message.Author as GuildMember)?.Nickname ?? message.Author.Username + "#" + message.Author.Discrim.ToString("0000")}.",
                LogSeverity.Verbose));

            client.InvokeMessageCreated(message);

            return Task.CompletedTask;
        }
    }
}