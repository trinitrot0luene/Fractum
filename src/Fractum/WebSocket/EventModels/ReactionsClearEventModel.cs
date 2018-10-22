using Newtonsoft.Json;

namespace Fractum.WebSocket.EventModels
{
    public class ReactionsClearEventModel : EventModelBase
    {
        [JsonProperty("message_id")]
        public ulong MessageId { get; private set; }

        [JsonProperty("channel_id")]
        public ulong ChannelId { get; private set; }

        [JsonProperty("guild_id")]
        public ulong? GuildId { get; private set; }
    }
}