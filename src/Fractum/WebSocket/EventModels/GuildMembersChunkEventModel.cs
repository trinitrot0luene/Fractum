using System.Collections.ObjectModel;
using Fractum.Entities;
using Fractum.Utilities;
using Fractum.WebSocket.Core;
using Newtonsoft.Json;

namespace Fractum.WebSocket.EventModels
{
    internal sealed class GuildMembersChunkEventModel : EventModelBase
    {
        [JsonProperty("guild_id")]
        public ulong GuildId { get; set; }

        [JsonProperty("members")]
        public ReadOnlyCollection<GuildMember> Members { get; private set; }

        public override void ApplyToCache(FractumCache cache)
        {
            if (cache.HasGuild(GuildId))
                foreach (var member in Members)
                    cache[GuildId].AddOrUpdate(member, old => old = member);
        }
    }
}