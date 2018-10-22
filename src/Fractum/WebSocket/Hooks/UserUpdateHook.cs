using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.Utilities;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class UserUpdateHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var eventArgs = (UserUpdateEventModel) args;

            var user = cache.GetUserOrDefault(eventArgs.Id);
            var clone = user?.Clone();

            if (user != null)
            {
                user.Discrim = eventArgs.Discrim;
                user.Username = eventArgs.Username;
                user.AvatarRaw = eventArgs.AvatarRaw;
            }

            client.InvokeUserUpdated(new CachedEntity<User>(clone as User), user);

            return Task.CompletedTask;
        }
    }
}