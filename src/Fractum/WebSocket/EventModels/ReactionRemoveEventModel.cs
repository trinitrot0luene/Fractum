using Fractum;
using Newtonsoft.Json;

namespace Fractum.WebSocket.EventModels
{
    public class ReactionRemoveEventModel : EventModelBase
    {
        [JsonProperty("user_id")]
        public ulong UserId { get; private set; }

        [JsonProperty("channel_id")]
        public ulong ChannelId { get; private set; }

        [JsonProperty("message_id")]
        public ulong MessageId { get; private set; }

        [JsonProperty("guild_id")]
        public ulong? GuildId { get; private set; }

        [JsonProperty("emoji")]
        public Emoji Emoji { get; private set; }
    }
}