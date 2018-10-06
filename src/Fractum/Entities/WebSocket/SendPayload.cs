using Newtonsoft.Json;

namespace Fractum.Entities.WebSocket
{
    internal sealed class SendPayload
    {
        [JsonProperty("op")]
        public OpCode op { get; set; }

        [JsonProperty("d")]
        public object d { get; set; }

        [JsonProperty("t", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string t { get; set; }

        [JsonProperty("seq", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int seq { get; set; }
    }
}