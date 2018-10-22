using Newtonsoft.Json;

namespace Fractum.WebSocket.EventModels
{
    public class RoleDeleteEventModel : EventModelBase
    {
        [JsonProperty("guild_id")]
        public ulong GuildId { get; private set; }

        [JsonProperty("role_id")]
        public ulong RoleId { get; private set; }
    }
}