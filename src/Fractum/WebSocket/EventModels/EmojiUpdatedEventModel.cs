using System.Collections.ObjectModel;
using Fractum.Entities;
using Fractum.WebSocket.Core;
using Newtonsoft.Json;

namespace Fractum.WebSocket.EventModels
{
    internal sealed class EmojiUpdatedEventModel : BaseEventModel
    {
        [JsonProperty("guild_id")]
        public ulong GuildId { get; private set; }

        [JsonProperty("emojis")]
        public ReadOnlyCollection<GuildEmoji> Emojis { get; private set; }

        public override void ApplyToCache(FractumCache cache)
        {
            throw new System.NotImplementedException();
        }
    }
}