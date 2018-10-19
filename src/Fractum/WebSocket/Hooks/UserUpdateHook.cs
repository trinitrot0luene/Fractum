using System.Linq;
using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.Utilities;
using Fractum.WebSocket.Core;
using Newtonsoft.Json.Linq;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class UserUpdateHook : IEventHook<JToken>
    {
        public Task RunAsync(JToken args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var user = args.ToObject<User>();

            var members = cache.Guilds.SelectMany(x => x.GetMembers()).Where(x => x.Id == user.Id);

            var clonedUser = members.FirstOrDefault()?.User.Clone() as User;

            foreach (var member in members)
                member.User = user;

            client.InvokeUserUpdated(new CachedEntity<User>(clonedUser), user);

            return Task.CompletedTask;
        }
    }
}