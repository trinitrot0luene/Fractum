using Newtonsoft.Json;

namespace Fractum.WebSocket.EventModels
{
    public class ChannelPinsUpdateEventModel : EventModelBase
    {
        [JsonProperty("channel_id")]
        public ulong ChannelId { get; private set; }
    }
}