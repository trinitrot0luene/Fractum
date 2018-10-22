using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.Entities.WebSocket;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class ChannelPinsUpdateHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var eventModel = (ChannelPinsUpdateEventModel) args;

            CachedTextChannel channel = null;

            foreach (var guild in cache.Guilds)
            {
                channel = guild.GetChannel(eventModel.ChannelId) as CachedTextChannel;
                if (channel != null)
                    break;
            }

            // TODO: DM Channels.

            client.InvokeLog(new LogMessage(nameof(ChannelPinsUpdateHook), $"Pins updated in channel {channel.Name}",
                LogSeverity.Debug));

            client.InvokeMessagePinned(channel);

            return Task.CompletedTask;
        }
    }
}