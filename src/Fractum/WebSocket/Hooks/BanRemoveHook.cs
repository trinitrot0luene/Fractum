using System.Linq;
using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;
using Newtonsoft.Json.Linq;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class BanRemoveHook : IEventHook<JToken>
    {
        public Task RunAsync(JToken args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var eventData = args.ToObject<GuildBanEventModel>();

            var guild = cache[eventData.GuildId];

            client.InvokeLog(new LogMessage(nameof(BanRemoveHook),
                $"{eventData.User} was unbanned in {guild?.Guild.Name ?? "Unknown Guild"}", LogSeverity.Info));

            client.InvokeMemberUnbanned(eventData.User);

            return Task.CompletedTask;
        }
    }
}