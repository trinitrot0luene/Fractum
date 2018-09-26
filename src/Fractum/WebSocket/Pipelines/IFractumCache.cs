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

        ConcurrentDictionary<ulong, Role> Roles { get; }

        ConcurrentDictionary<ulong, GuildMember> Members { get; }

        ConcurrentDictionary<ulong, GuildChannel> Channels { get; }

        Guild PopulateGuild(ulong guildId);

        GuildMember PopulateMember(ulong guildId, ulong memberId);

        GuildChannel PopulateChannel(ulong channelId);

        void Initialise();

        void Reset();
    }
}
