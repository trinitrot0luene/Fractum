using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fractum.Entities.WebSocket
{
    public sealed class Payload : IDisposable
    {
        [JsonProperty("op")]
        public OpCode OpCode { get; set; }

        [JsonProperty("d", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public JToken Data;

        [JsonProperty("s", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int? Seq { get; set; }

        [JsonProperty("t", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Type { get; set; }

        public void Dispose()
        {
            Data = null;
        }
    }
}