using Fractum.WebSocket;
using Fractum.WebSocket.EventModels;
using Newtonsoft.Json;

namespace Fractum.Entities.WebSocket
{
    public sealed class Payload<T> : Payload, IPayload<T> where T : EventModelBase
    {
        internal Payload()
        {
        }

        [JsonProperty("d", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public T Data { get; set; }

        [JsonProperty("s", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int? Seq { get; set; }
    }

    public class Payload
    {
        [JsonProperty("op")]
        public OpCode OpCode { get; set; }

        [JsonProperty("t", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Type { get; set; }
    }
}