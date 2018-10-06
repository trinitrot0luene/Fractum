using Newtonsoft.Json;

namespace Fractum.Entities
{
    public sealed class Emoji : DiscordEntity
    {
        [JsonProperty("name")]
        public string Name { get; internal set; }

        [JsonProperty("animated")]
        public bool IsAnimated { get; internal set; }
    }
}