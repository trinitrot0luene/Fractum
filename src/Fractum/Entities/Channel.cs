using Newtonsoft.Json;

namespace Fractum.Entities
{
    public class Channel : DiscordEntity
    {
        [JsonProperty("type")]
        public ChannelType Type { get; private set; }

        [JsonProperty("name")]
        public string Name { get; private set; }
    }
}