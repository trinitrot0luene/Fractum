using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.Entities.WebSocket;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class ChannelPinsUpdateHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, ISession session)
        {
            var eventModel = (ChannelPinsUpdateEventModel) args;

            CachedGuildChannel channel = null;
            foreach (var guild in cache.Guilds)
                if (guild.TryGet(eventModel.ChannelId, out channel))
                    break;


            // TODO: DM Channels.

            cache.Client.InvokeLog(new LogMessage(nameof(ChannelPinsUpdateHook), $"Pins updated in channel {channel.Name}",
                LogSeverity.Debug));

            cache.Client.InvokeMessagePinned(channel as CachedTextChannel);

            return Task.CompletedTask;
        }
    }
}