using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Fractum.Rest
{
    public sealed class RestGuild : RestEntity, IGuild
    {
        [JsonProperty("application_id")]
        public ulong? ApplicationId { get; private set; }

        [JsonProperty("afk_timeout")]
        public int AfkTimeout { get; private set; }

        [JsonProperty("default_message_notifications")]
        public MessageNotificationLevel MessageNotificationLevel { get; private set; }

        [JsonProperty("explicit_content_filter")]
        public ExplicitContentFilterLevel ExplicitContentFilterLevel { get; private set; }

        [JsonProperty("max_presences")]
        public int? MaxPresences { get; private set; }

        [JsonProperty("verification_level")]
        public VerificationLevel VerificationLevel { get; private set; }

        [JsonProperty("Widget_channel_id")]
        public ulong? WidgetChannelId { get; private set; }

        [JsonProperty("embed_channel_id")]
        public ulong? EmbedChannelId { get; private set; }

        [JsonProperty("splash")]
        private string SplashHash { get; set; }

        [JsonProperty("emojis")]
        public GuildEmoji[] Emojis { get; private set; }

        [JsonProperty("embed_enabled")]
        public bool EmbedEnabled { get; private set; }

        [JsonProperty("owner_id")]
        public ulong OwnerId { get; private set; }

        [JsonProperty("mfa_level")]
        public MfaLevel MfaLevel { get; private set; }

        [JsonProperty("system_channel_id")]
        public ulong? SystemChannelId { get; private set; }

        [JsonProperty("widget_enabled")]
        public bool WidgetEnabled { get; private set; }

        [JsonProperty("icon")]
        private string IconHash { get; set; }

        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("roles")]
        public Role[] Roles { get; private set; }

        [JsonProperty("region")]
        public string Region { get; private set; }

        [JsonProperty("max_members")]
        public int MaxMembers { get; private set; }

        public Task DeleteAsync()
            => Client.DeleteGuildAsync(Id);

        public string GetIconUrl() => IconHash == null
            ? default
            : string.Concat(Consts.CDN, string.Format(Consts.CDN_GUILD_ICON, Id.ToString(), IconHash, ".png"));

        public string GetSplashUrl() => SplashHash == null
            ? default
            : string.Concat(Consts.CDN, string.Format(Consts.CDN_GUILD_SPLASH, Id.ToString(), SplashHash, ".png"));

    }
}