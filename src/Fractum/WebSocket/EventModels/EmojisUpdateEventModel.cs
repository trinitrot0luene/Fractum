using System.Collections.ObjectModel;
using Fractum.Entities;
using Newtonsoft.Json;

namespace Fractum.WebSocket.EventModels
{
    internal sealed class EmojisUpdateEventModel : EventModelBase
    {
        [JsonProperty("guild_id")]
        public ulong GuildId { get; private set; }

        [JsonProperty("emojis")]
        public ReadOnlyCollection<GuildEmoji> Emojis { get; private set; }
    }
}