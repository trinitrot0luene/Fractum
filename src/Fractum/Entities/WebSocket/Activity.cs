using Newtonsoft.Json;

namespace Fractum.Entities.WebSocket
{
    public sealed class Activity
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public ActivityType Type { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("details", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Details { get; set; }

        [JsonProperty("state", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string State { get; set; }
    }
}