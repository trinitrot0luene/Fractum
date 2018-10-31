using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fractum.Entities.WebSocket;

namespace Fractum.WebSocket
{
    public sealed class KeyedChannelWrapper : IKeyedEnumerable<ulong, CachedGuildChannel>
    {
        private readonly FractumCache _cache;

        internal KeyedChannelWrapper(FractumCache cache)
            => _cache = cache;

        public CachedGuildChannel this[ulong key]
        {
            get
            {
                foreach (var guild in _cache.Guilds)
                {
                    if (guild.TryGet(key, out CachedGuildChannel channel))
                        return channel;
                }

                throw new KeyNotFoundException("The channel could not be found in cache.");
            }
        }

        public bool TryGetValue(ulong key, out CachedGuildChannel value)
        {
            foreach (var guild in _cache.Guilds)
                if (guild.TryGet(key, out value))
                    return true;

            value = default;

            return false;
        }

        public IEnumerator<CachedGuildChannel> GetEnumerator() =>
            _cache.Guilds.Select(x => x.Guild).SelectMany(x => x.Channels).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}