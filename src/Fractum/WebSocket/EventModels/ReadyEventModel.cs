using System.Collections.ObjectModel;
using Fractum.Entities;
using Newtonsoft.Json;

namespace Fractum.WebSocket.EventModels
{
    internal class ReadyEventModel : EventModelBase
    {
        [JsonProperty("v")]
        public int ProtoVersion { get; private set; }

        [JsonProperty("user")]
        public User Owner { get; private set; }

        [JsonProperty("guilds")]
        public ReadOnlyCollection<GuildCreateEventModel> guilds { get; private set; }

        [JsonProperty("session_id")]
        public string SessionId { get; private set; }

        [JsonProperty("_trace")]
        public string[] Trace { get; private set; }
    }
}