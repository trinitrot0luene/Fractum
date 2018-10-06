using System.Linq;
using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.Pipelines;
using Newtonsoft.Json.Linq;

namespace Fractum.WebSocket.Hooks
{
    public class ChannelPinsUpdateHook : IEventHook<JToken>
    {
        public Task RunAsync(JToken args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var channel = cache.Guilds
                .SelectMany(kvp => kvp.Value.Channels)
                .Select(cc => cc.Value)
                .FirstOrDefault(c => c.Id == args.Value<ulong>("channel_id"));

            client.InvokeMessagePinned(channel as TextChannel);
            client.InvokeLog(new LogMessage(nameof(ChannelPinsUpdateHook), $"Pins updated in channel {channel.Name}",
                LogSeverity.Debug));

            return Task.CompletedTask;
        }
    }
}