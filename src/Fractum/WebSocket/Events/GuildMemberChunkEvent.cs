using Fractum.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Fractum.WebSocket.Events
{
    internal sealed class GuildMemberChunkEvent
    {
        [JsonProperty("guild_id")]
        private string GuildIdRaw { get; set; }

        [JsonIgnore]
        public ulong GuildId { get => ulong.Parse(GuildIdRaw); }

        [JsonProperty("members")]
        public ReadOnlyCollection<GuildMember> Members { get; private set; }
    }
}
