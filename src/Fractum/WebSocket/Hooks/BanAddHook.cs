using System.Linq;
using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;
using Newtonsoft.Json.Linq;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class BanAddHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var eventData = args.Cast<BanAddEventModel>();

            var guild = cache[eventData.GuildId];
            var member = guild.GetMembers().First(x => x.Id == eventData.User.Id);

            client.InvokeLog(new LogMessage(nameof(BanAddHook),
                $"{eventData.User.ToString()} was banned in {guild.Guild.Name}", LogSeverity.Info));

            client.InvokeMemberBanned(member);

            return Task.CompletedTask;
        }
    }
}