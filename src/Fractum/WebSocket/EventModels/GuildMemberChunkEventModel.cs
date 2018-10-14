﻿using System.Collections.ObjectModel;
using Fractum.Entities;
using Fractum.Utilities;
using Fractum.WebSocket.Core;
using Newtonsoft.Json;

namespace Fractum.WebSocket.EventModels
{
    internal sealed class GuildMemberChunkEventModel : BaseEventModel
    {
        [JsonProperty("guild_id")]
        public ulong GuildId { get; set; }

        [JsonProperty("members")]
        public ReadOnlyCollection<GuildMember> Members { get; private set; }

        public override void ApplyToCache(FractumCache cache)
        {
            if (cache.Guilds.TryGetValue(GuildId, out var guildCache))
                foreach (var member in Members)
                    guildCache.Members.AddOrUpdate((a, b) => a.Id == b.Id, member, oldMember => oldMember = member ?? oldMember);
        }
    }
}