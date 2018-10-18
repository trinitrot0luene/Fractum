using System.Linq;
using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.WebSocket.Core;
using Newtonsoft.Json.Linq;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class MessageReceivedHook : IEventHook<JToken>
    {
        public Task RunAsync(JToken args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var message = args.ToObject<Message>();

            if (message.GuildId.HasValue && cache.HasGuild(message.GuildId.Value))
            {
                var guild = cache[message.GuildId.Value];
                guild.AddOrCreate(message);
            }
            else return Task.CompletedTask;

            client.InvokeLog(new LogMessage(nameof(MessageReceivedHook),
                $"Received message from {(message.Author as GuildMember)?.Nickname ?? message.Author.Username + "#" + message.Author.Discrim.ToString("0000")}.",
                LogSeverity.Verbose));

            client.InvokeMessageCreated(message);

            return Task.CompletedTask;
        }
    }
}