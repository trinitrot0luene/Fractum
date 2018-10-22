using Newtonsoft.Json;

namespace Fractum.WebSocket.EventModels
{
    public class MessageDeleteEventModel : EventModelBase
    {
        [JsonProperty("id")]
        public ulong Id { get; private set; }

        [JsonProperty("channel_id")]
        public ulong ChannelId { get; private set; }

        [JsonProperty("guild_id")]
        public ulong? GuildId { get; private set; }
    }
}