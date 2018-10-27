using System.Collections;
using System.Collections.Generic;
using Fractum.Entities.WebSocket;

namespace Fractum.WebSocket
{
    public sealed class KeyedGuildWrapper : IKeyedEnumerable<ulong, CachedGuild>
    {
        private readonly ISocketCache<ISyncedGuild> _cache;

        public KeyedGuildWrapper(ISocketCache<ISyncedGuild> cache)
            => _cache = cache;

        public CachedGuild this[ulong key] => _cache.TryGetGuild(key, out var guild) ? guild.Guild : throw new KeyNotFoundException();

        public bool TryGetValue(ulong key, out CachedGuild value)
        {
            if (_cache.TryGetGuild(key, out var guild))
            {
                value = guild.Guild;
                return true;
            }

            value = default;

            return false;
        }

        public IEnumerator<CachedGuild> GetEnumerator()
        {
            foreach (var guild in _cache.Guilds)
                yield return guild.Guild;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}