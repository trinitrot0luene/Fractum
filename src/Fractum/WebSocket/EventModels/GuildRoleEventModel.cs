using Fractum.Entities;
using Newtonsoft.Json;

namespace Fractum.WebSocket.EventModels
{
    public class GuildRoleEventModel
    {
        [JsonProperty("guild_id")]
        public ulong GuildId { get; private set; }

        [JsonProperty("role_id")]
        public ulong? RoleId { get; private set; }

        [JsonProperty("role")]
        public Role Role { get; private set; }
    }
}