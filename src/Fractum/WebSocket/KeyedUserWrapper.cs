using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fractum.Entities;

namespace Fractum.WebSocket
{
    public sealed class KeyedUserWrapper : IKeyedEnumerable<ulong, User>
    {
        private readonly FractumCache _cache;

        internal KeyedUserWrapper(FractumCache cache)
            => _cache = cache;

        public User this[ulong key] => _cache.TryGetUser(key, out var user) ? user : throw new KeyNotFoundException("The user could not be found in cache.");

        public bool TryGetValue(ulong key, out User value) => _cache.TryGetUser(key, out value);

        public IEnumerator<User> GetEnumerator() => _cache.Users.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}