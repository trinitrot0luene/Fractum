using Fractum.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Fractum.WebSocket.Pipelines
{
    public sealed class FractumCache : IFractumCache
    {
        public ConcurrentDictionary<ulong, Guild> Guilds { get; private set; }

        public FractumSocketConfig Config { get; private set; }

        public FractumCache(FractumSocketConfig config)
        {
            Guilds = new ConcurrentDictionary<ulong, Guild>();
            Config = config;
        }

        public void Clear() => Guilds = new ConcurrentDictionary<ulong, Guild>();

        public void UpdateGuild(ulong guildId, Action<Guild> updateGuild)
        {
            if (Guilds.TryGetValue(guildId, out var guild))
                updateGuild(guild);
        }

        public Guild GetGuild(ulong guildId)
            => Guilds.TryGetValue(guildId, out var guild) ? guild : default;
    }
}
