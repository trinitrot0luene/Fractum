using Newtonsoft.Json;

namespace Fractum
{
    public sealed class VoiceChannelProperties : GuildChannelProperties
    {
        internal VoiceChannelProperties()
        {
        }

        [JsonProperty("bitrate", NullValueHandling = NullValueHandling.Ignore)]
        public int? Bitrate { get; set; }

        [JsonProperty("user_limit", NullValueHandling = NullValueHandling.Ignore)]
        public int? UserLimit { get; set; }

        [JsonProperty("parent_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? ParentId { get; set; }
    }
}