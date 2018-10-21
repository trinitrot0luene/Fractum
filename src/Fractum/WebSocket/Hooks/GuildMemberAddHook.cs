using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;
using Newtonsoft.Json.Linq;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class GuildMemberAddHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var member = args.Cast<GuildMemberAddEventModel>();

            var user = args.Value<User>("user");

            cache.AddUser(user);

            cache[member.GuildId.Value]?.AddOrUpdate(member, old => old = member);

            client.InvokeMemberJoined(member);

            return Task.CompletedTask;
        }
    }
}