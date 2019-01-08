using System.Collections;
using System.Collections.Generic;
using Fractum.WebSocket;

namespace Fractum.WebSocket
{
    public sealed class KeyedGuildWrapper : IKeyedEnumerable<ulong, CachedGuild>
    {
        private readonly GatewayCache _cache;

        internal KeyedGuildWrapper(GatewayCache cache)
            => _cache = cache;

        public CachedGuild this[ulong key] => _cache.TryGetGuild(key, out var guild) ? guild.Guild : throw new KeyNotFoundException("The guild could not be found in cache.");

        public bool TryGetValue(ulong key, out CachedGuild value)
        {
            if (_cache.TryGetGuild(key, out var cachedGuild))
            {
                value = cachedGuild.Guild;
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