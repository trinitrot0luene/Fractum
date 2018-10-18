using Fractum.Entities;
using Fractum.WebSocket.Core;
using Newtonsoft.Json;

namespace Fractum.WebSocket.EventModels
{
    internal sealed class GuildBanEventModel : BaseEventModel
    {
        [JsonProperty("guild_id")]
        public ulong GuildId { get; private set; }

        [JsonProperty("user")]
        public User User { get; private set; }

        public override void ApplyToCache(FractumCache cache)
        {
            throw new System.NotImplementedException();
        }
    }
}