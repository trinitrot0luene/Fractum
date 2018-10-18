using System.Linq;
using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.WebSocket.Core;
using Newtonsoft.Json.Linq;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class ChannelPinsUpdateHook : IEventHook<JToken>
    {
        public Task RunAsync(JToken args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var channel = cache.Guilds
                .SelectMany(caches => caches.GetChannels())
                .FirstOrDefault(c => c.Id == args.Value<ulong>("channel_id"));

            client.InvokeLog(new LogMessage(nameof(ChannelPinsUpdateHook), $"Pins updated in channel {channel.Name}",
                LogSeverity.Debug));

            client.InvokeMessagePinned(channel as TextChannel);

            return Task.CompletedTask;
        }
    }
}