using Newtonsoft.Json;

namespace Fractum.Rest
{
    public sealed class PartialChannel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public ChannelType Type { get; set; }

        public PartialChannel(string name, ChannelType type = ChannelType.GuildText)
        {
            Name = name;
            Type = type;
        }
    }
}