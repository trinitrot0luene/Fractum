using Fractum.Entities;
using Fractum.WebSocket.Pipelines;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Fractum.WebSocket.Events
{
    internal sealed class GuildMemberChunkEvent : IEvent
    {
        [JsonProperty("guild_id")]
        public ulong GuildId { get; set; }

        [JsonProperty("members")]
        public ReadOnlyCollection<GuildMember> Members { get; private set; }

        public void ApplyToCache(FractumCache cache)
        {
            if (cache.Guilds.TryGetValue(GuildId, out var guildCache))
                foreach (var member in Members)
                    guildCache.Members.AddOrUpdate(member.Id, member, (k, v) => v = member ?? v);
        }
    }
}
