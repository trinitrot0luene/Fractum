using System.Collections.Generic;
using Newtonsoft.Json;

namespace Fractum.WebSocket.EventModels
{
    internal sealed class GuildMembersChunkEventModel : EventModelBase
    {
        [JsonProperty("guild_id")]
        public ulong GuildId { get; set; }

        [JsonProperty("members")]
        public List<GuildMemberAddEventModel> Members { get; private set; }
    }
}