using Fractum;
using Newtonsoft.Json;

namespace Fractum.WebSocket.EventModels
{
    public sealed class GuildEmojiModel
    {
        [JsonProperty("id")]
        public ulong Id { get; private set; }

        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("roles")]
        public ulong[] RoleIds { get; private set; }

        [JsonProperty("user")]
        public User Creator { get; private set; }

        [JsonProperty("require_colons")]
        public bool RequiresColons { get; private set; }

        [JsonProperty("managed")]
        public bool IsManaged { get; private set; }

        [JsonProperty("animated")]
        public bool IsAnimated { get; private set; }
    }
}