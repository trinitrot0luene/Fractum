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

            cache.PopulateMessage(message);
            if (message.Channel is TextChannel txtChn)
            cache.UpdateGuildCache(txtChn.Guild.Id, gc =>
            {
                if (gc.Channels.FirstOrDefault(c => c.Id == message.ChannelId) is GuildChannel chn &&
                    gc.Messages.TryGetValue(message.ChannelId, out var mc))
                {
                    (chn as TextChannel).LastMessageId = message.Id;

                    mc.Add(message);
                }
            });

            client.InvokeLog(new LogMessage(nameof(MessageReceivedHook),
                $"Received message from {(message.Author as GuildMember)?.Nickname ?? message.Author.Username + "#" + message.Author.Discrim.ToString("0000")}.",
                LogSeverity.Verbose));

            client.InvokeMessageCreated(message);

            return Task.CompletedTask;
        }
    }
}