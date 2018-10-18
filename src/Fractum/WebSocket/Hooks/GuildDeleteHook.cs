using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;
using Newtonsoft.Json.Linq;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class GuildDeleteHook : IEventHook<JToken>
    {
        public Task RunAsync(JToken args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var model = args.ToObject<GuildCreateEventModel>();

            var guild = cache[model.Id];

            cache.Remove(model.Id);

            client.InvokeLog(new LogMessage(nameof(GuildDeleteHook), $"Guild unavailable: {guild?.Guild.Name}",
                LogSeverity.Info));

            client.InvokeGuildUnavailable(guild.Guild);
            
            return Task.CompletedTask;
        }
    }
}