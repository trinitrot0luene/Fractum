using Newtonsoft.Json;

namespace Fractum.Entities.Properties
{
    public sealed class VoiceChannelProperties : GuildChannelProperties
    {
        [JsonProperty("bitrate", NullValueHandling = NullValueHandling.Ignore)]
        public int? Bitrate { get; set; }

        [JsonProperty("user_limit", NullValueHandling = NullValueHandling.Ignore)]
        public int? UserLimit { get; set; }

        [JsonProperty("parent_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? ParentId { get; set; }
    }
}