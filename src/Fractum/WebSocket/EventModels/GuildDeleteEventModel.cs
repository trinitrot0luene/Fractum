using Newtonsoft.Json;

namespace Fractum.WebSocket.EventModels
{
    public class GuildDeleteEventModel : EventModelBase
    {
        [JsonProperty("id")]
        public ulong Id { get; private set; }
    }
}