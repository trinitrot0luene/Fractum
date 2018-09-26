using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fractum.Entities
{
    public sealed class GatewayBotResponse
    {
        [JsonProperty("url")]
        public string Url { get; private set; }

        [JsonProperty("shards")]
        public int Shards { get; private set; }

        [JsonProperty("session_start_limit")]
        public Dictionary<string, int> SessionStartLimit { get; private set; }
    }
}
