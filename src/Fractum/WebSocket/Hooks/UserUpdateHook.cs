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

            var oldUser = cache.GetUserOrDefault(user.Id).Clone();

            cache.AddUser(user);

            client.InvokeUserUpdated(new CachedEntity<User>(oldUser as User), user);

            return Task.CompletedTask;
        }
    }
}