using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.WebSocket.Core;
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
            cache.UpdateGuildCache(message.Guild.Id, gc =>
            {
                if (gc.Channels.TryGetValue(message.ChannelId, out var chn) &&
                    gc.Messages.TryGetValue(message.ChannelId, out var mc))
                    mc.Add(message);
            });

            client.InvokeLog(new LogMessage(nameof(MessageReceivedHook),
                $"Received message from {(message.Author as GuildMember)?.Nickname ?? message.Author.Username + "#" + message.Author.Discrim.ToString("0000")}.",
                LogSeverity.Verbose));

            client.InvokeMessageCreated(message);

            return Task.CompletedTask;
        }
    }
}