using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class UserUpdateHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, ISocketCache<ISyncedGuild> cache, ISession session)
        {
            var eventArgs = (UserUpdateEventModel) args;

            if (cache.TryGetUser(eventArgs.Id, out var user))
            {
                var clone = user?.Clone();

                if (user != null)
                {
                    user.DiscrimValue = eventArgs.Discrim;
                    user.Username = eventArgs.Username;
                    user.AvatarRaw = eventArgs.AvatarRaw;
                }

                cache.Client.InvokeUserUpdated(new CachedEntity<User>(clone as User), user);
            }

            return Task.CompletedTask;
        }
    }
}