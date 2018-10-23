using Newtonsoft.Json;

namespace Fractum.Entities
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

        public override string ToString() 
            => $"{(Type == ActivityType.Listening ? "Listening to" : Type == ActivityType.Playing ? "Playing" : Type == ActivityType.Streaming ? "Streaming" : Type.ToString())}{(Name != null ? $" {Name}": string.Empty)}{(State != null ? $", {State}" : string.Empty)}{(Details != null ? $", {Details}" : string.Empty)}";
    }
}