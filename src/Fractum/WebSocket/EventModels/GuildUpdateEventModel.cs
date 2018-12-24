using System.Collections.Generic;
using Fractum;
using Newtonsoft.Json;

namespace Fractum.WebSocket.EventModels
{
    public class GuildUpdateEventModel : EventModelBase
    {
        [JsonProperty("roles")]
        public List<Role> Roles { get; private set; }

        [JsonProperty("emojis")]
        public List<GuildEmoji> Emojis { get; private set; }

        #region Properties

        [JsonProperty("id")]
        public ulong Id { get; private set; }

        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("icon")]
        public string Icon { get; private set; }

        [JsonProperty("splash")]
        public string Splash { get; private set; }

        [JsonProperty("owner_id")]
        public ulong OwnerId { get; private set; }

        [JsonProperty("region")]
        public string Region { get; private set; }

        [JsonProperty("afk_channel_id")]
        public ulong? AfkChannelId { get; private set; }

        [JsonProperty("afk_timeout")]
        public int AfkTimeout { get; private set; }

        [JsonProperty("embed_enabled")]
        public bool? EmbedEnabled { get; private set; }

        [JsonProperty("embed_channel_id")]
        public ulong? EmbedChannelId { get; private set; }

        [JsonProperty("verification_level")]
        public int VerificationLevel { get; private set; }

        [JsonProperty("default_message_notifications")]
        public int DefaultMessageNotifications { get; private set; }

        [JsonProperty("explicit_content_filter")]
        public int ExplicitContentFilter { get; private set; }

        [JsonProperty("mfa_level")]
        public bool RequireMfa { get; private set; }

        [JsonProperty("unavailable")]
        public bool IsUnavailable { get; private set; }

        [JsonProperty("member_count")]
        public int MemberCount { get; private set; }

        [JsonProperty("lazy")]
        public bool Lazy { get; private set; }

        [JsonProperty("large")]
        public bool Large { get; private set; }

        #endregion
    }
}