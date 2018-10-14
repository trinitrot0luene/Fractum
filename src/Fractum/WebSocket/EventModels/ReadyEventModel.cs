using System.Collections.ObjectModel;
using Fractum.Entities;
using Fractum.WebSocket.Core;
using Newtonsoft.Json;

namespace Fractum.WebSocket.EventModels
{
    internal sealed class ReadyEventModel : BaseEventModel
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
        public ulong[] Trace { get; private set; }

        public override void ApplyToCache(FractumCache cache)
        {
            foreach (var guild in guilds)
                cache.Guilds[guild.Id] = new GuildCache(cache.Client, guild);
        }
    }
}