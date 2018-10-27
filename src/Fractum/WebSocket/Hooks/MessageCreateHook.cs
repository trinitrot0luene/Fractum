using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.Entities.WebSocket;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class MessageCreateHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, ISocketCache<ISyncedGuild> cache, ISession session)
        {
            var eventModel = (MessageCreateEventModel) args;

            if (cache.TryGetGuild(eventModel.ChannelId, out var guild, SearchType.Channel))
            {
                var message = new CachedMessage(cache, eventModel);

                guild.AddOrReplace(message);

                cache.Client.InvokeLog(new LogMessage(nameof(MessageCreateHook),
                    $"Received message from {(message.Author as CachedMember)?.Nickname ?? message.Author.Username + "#" + message.Author.DiscrimValue.ToString("0000")}.",
                    LogSeverity.Verbose));

                cache.Client.InvokeMessageCreated(message);
            }

            return Task.CompletedTask;
        }
    }
}