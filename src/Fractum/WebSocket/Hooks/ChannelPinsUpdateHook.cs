using System.Threading.Tasks;
using Fractum;
using Fractum.WebSocket;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class ChannelPinsUpdateHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, GatewayCache cache, GatewaySession session)
        {
            var eventModel = (ChannelPinsUpdateEventModel) args;

            if (cache.Client.Channels.TryGetValue(eventModel.ChannelId, out var guildChannel))
            {
                cache.Client.InvokeLog(new LogMessage(nameof(ChannelPinsUpdateHook), $"Pins updated in channel {guildChannel.Name}",
                LogSeverity.Debug));

                cache.Client.InvokeMessagePinned(guildChannel as CachedTextChannel);
            }
            else
                return Task.CompletedTask;

            return Task.CompletedTask;
        }
    }
}