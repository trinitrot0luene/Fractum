using Fractum;
using Newtonsoft.Json;

namespace Fractum.WebSocket.EventModels
{
    public class RoleUpdateEventModel : EventModelBase
    {
        [JsonProperty("guild_id")]
        public ulong GuildId { get; private set; }

        [JsonProperty("role")]
        public Role Role { get; private set; }
    }
}