using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.WebSocket.Core;
using Newtonsoft.Json.Linq;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class GuildMemberAddHook : IEventHook<JToken>
    {
        public Task RunAsync(JToken args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var member = args.ToObject<GuildMember>();

            cache[member.GuildId.Value]?.AddOrUpdate(member, old => old = member);

            client.InvokeMemberJoined(member);

            return Task.CompletedTask;
        }
    }
}