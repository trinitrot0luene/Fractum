using Newtonsoft.Json;

namespace Fractum.Entities
{
    public sealed class GuildEmoji : DiscordEntity
    {
        [JsonProperty("name")]
        public string Name { get; internal set; }

        [JsonProperty("roles")]
        public ulong[] RoleIds { get; internal set; }

        [JsonProperty("user")]
        public User Creator { get; internal set; }

        [JsonProperty("require_colons")]
        public bool RequiresColons { get; internal set; }

        [JsonProperty("managed")]
        public bool IsManaged { get; internal set; }

        [JsonProperty("animated")]
        public bool IsAnimated { get; internal set; }
    }
}