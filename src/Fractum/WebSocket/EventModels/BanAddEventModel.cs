using Fractum.Entities;
using Newtonsoft.Json;

namespace Fractum.WebSocket.EventModels
{
    public class BanAddEventModel : EventModelBase
    {
        [JsonProperty("user")]
        public User User { get; private set; }

        [JsonProperty("guild_id")]
        public ulong GuildId { get; private set; }
    }
}