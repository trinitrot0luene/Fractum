using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.WebSocket.Pipelines;
using Newtonsoft.Json.Linq;

namespace Fractum.WebSocket.Hooks
{
    public sealed class MessageReceivedHook : IEventHook<JToken>
    {
        public Task RunAsync(JToken args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var message = args.ToObject<Message>();

            cache.AddAndPopulateMessage(message);

            client.InvokeLog(new LogMessage(nameof(MessageReceivedHook),
                $"Received message from {(message.Author as GuildMember)?.Nickname ?? message.Author.Username + "#" + message.Author.Discrim.ToString("0000")}.",
                LogSeverity.Verbose));

            return Task.CompletedTask;
        }
    }
}