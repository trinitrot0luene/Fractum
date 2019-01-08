using Fractum.WebSocket;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Fractum.WebSocket
{
    public sealed class KeyedDMChannelWrapper : IKeyedEnumerable<ulong, CachedDMChannel>
    {
        private readonly GatewayCache _cache;

        internal KeyedDMChannelWrapper(GatewayCache cache)
            => _cache = cache;

        public CachedDMChannel this[ulong key] => _cache.TryGetDmChannel(key, out var channel) ? channel : throw new KeyNotFoundException("The channel could not be found in cache.");

        public bool TryGetValue(ulong key, out CachedDMChannel value) => _cache.TryGetDmChannel(key, out value);

        public IEnumerator<CachedDMChannel> GetEnumerator() => _cache.DmChannels.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
