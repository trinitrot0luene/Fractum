using Newtonsoft.Json;

namespace Fractum.Entities
{
    public sealed class TextChannelProperties : GuildChannelProperties
    {
        internal TextChannelProperties()
        {
        }

        [JsonProperty("topic", NullValueHandling = NullValueHandling.Ignore)]
        public string Topic { get; set; }

        [JsonProperty("nsfw", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsNsfw { get; set; }

        [JsonProperty("rate_limit_per_user", NullValueHandling = NullValueHandling.Ignore)]
        public int? PerUserRatelimit { get; set; }

        [JsonProperty("parent_id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? ParentId { get; set; }
    }
}