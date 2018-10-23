using Newtonsoft.Json;

namespace Fractum.WebSocket.EventModels
{
    public class IntegrationsUpdatedEventModel : EventModelBase
    {
        [JsonProperty("guild_id")]
        public ulong GuildId { get; private set; }
    }
}