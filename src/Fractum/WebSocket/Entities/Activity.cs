using Newtonsoft.Json;
using System;

namespace Fractum.WebSocket.Entities
{
    public sealed class Activity
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public ActivityType Type { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}