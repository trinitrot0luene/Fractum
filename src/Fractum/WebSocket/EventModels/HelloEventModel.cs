using Newtonsoft.Json;

namespace Fractum.WebSocket.EventModels
{
    public class HelloEventModel : EventModelBase
    {
        [JsonProperty("heartbeat_interval")]
        public int HeartbeatInterval { get; private set; }
    }
}