using System.Collections;
using System.Collections.Generic;
using Fractum.Entities;

namespace Fractum.WebSocket
{
    public class KeyedUserWrapper : IKeyedEnumerable<ulong, User>
    {
        private readonly ISocketCache<ISyncedGuild> _cache;

        public KeyedUserWrapper(ISocketCache<ISyncedGuild> cache)
            => _cache = cache;

        public User this[ulong key]
        {
            get => _cache.TryGetUser(key, out var user) ? user : throw new KeyNotFoundException();
        }

        public bool TryGetValue(ulong key, out User value) => _cache.TryGetUser(key, out value);

        public IEnumerator<User> GetEnumerator() => _cache.Users.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}