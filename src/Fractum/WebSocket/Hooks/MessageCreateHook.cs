using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.Entities.WebSocket;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class MessageCreateHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var eventModel = (MessageCreateEventModel) args;

            if (eventModel.GuildId.HasValue && cache.HasGuild(eventModel.GuildId.Value))
            {
                var guild = cache[eventModel.GuildId.Value];

                var message = new CachedMessage(cache, eventModel);

                guild.AddOrCreate(message);

                client.InvokeLog(new LogMessage(nameof(MessageCreateHook),
                    $"Received message from {(message.Author as CachedMember)?.Nickname ?? message.Author.Username + "#" + message.Author.DiscrimValue.ToString("0000")}.",
                    LogSeverity.Verbose));

                client.InvokeMessageCreated(message);
            }

            return Task.CompletedTask;
        }
    }
}