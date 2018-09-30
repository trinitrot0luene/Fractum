using Fractum.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Fractum.WebSocket.Pipelines
{
    /// <summary>
    /// Provides entity-level caching of event payloads.
    /// </summary>
    public interface IFractumCache
    {
        ConcurrentDictionary<ulong, Guild> Guilds { get; }

        FractumSocketConfig Config { get; }

        Guild GetGuild(ulong guildId);

        void UpdateGuild(ulong guildId, Action<Guild> updateGuild);

        void Clear();
    }
}
