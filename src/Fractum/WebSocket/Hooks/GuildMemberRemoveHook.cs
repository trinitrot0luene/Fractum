using System.Linq;
using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;
using Newtonsoft.Json.Linq;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class GuildMemberRemoveHook : IEventHook<JToken>
    {
        public Task RunAsync(JToken args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var eventArgs = args.ToObject<GuildMemberRemovedEventModel>();

            var member = cache[eventArgs.GuildId]?.GetMembers().First(x => x.Id == eventArgs.User.Id);

            if (member != null)
                cache[eventArgs.GuildId]?.Remove(member);

            client.InvokeMemberLeft(member as IUser ?? eventArgs.User);

            return Task.CompletedTask;
        }
    }
}