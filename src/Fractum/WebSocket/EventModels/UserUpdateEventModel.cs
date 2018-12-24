using Fractum.WebSocket;
using Newtonsoft.Json;

namespace Fractum.WebSocket.EventModels
{
    public class UserUpdateEventModel : EventModelBase
    {
        [JsonProperty("id")]
        public ulong Id { get; private set; }

        [JsonProperty("avatar")]
        public string AvatarRaw { get; set; }

        [JsonProperty("member", NullValueHandling = NullValueHandling.Ignore)]
        public PartialMember Member { get; private set; }

        [JsonProperty("username")]
        public string Username { get; internal set; }

        [JsonProperty("discriminator")]
        public short Discrim { get; internal set; }

        [JsonProperty("bot")]
        public bool IsBot { get; private set; }
    }
}