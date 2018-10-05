using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fractum.WebSocket.Entities
{
    public sealed class Payload
    {
        [JsonProperty("op")]
        public OpCode OpCode { get; set; }

        [JsonProperty("d")]
        public JToken Data { get; set; }

        [JsonIgnore]
        public string DataValue { set => Data = JToken.Parse(value); }

        [JsonIgnore]
        public JObject DataObject { get => Data as JObject; }

        [JsonIgnore]
        public JArray DataArray { get => Data as JArray; }

        [JsonProperty("s", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int? Seq { get; set; }

        [JsonProperty("t", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Type { get; set; }
    }
}
