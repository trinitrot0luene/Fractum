using Newtonsoft.Json;

namespace Fractum.Rest
{
    public sealed class GuildCreationArguments
    {
        [JsonProperty("name")]
        public string Name { get; }

        [JsonProperty("region")]
        public string VoiceRegionId { get; }

        [JsonProperty("icon")]
        public string IconBase64 { get; }

        [JsonProperty("verification_level")]
        public VerificationLevel VerificationLevel { get; }

        [JsonProperty("default_message_notifications")]
        public MessageNotificationLevel MessageNotificationLevel { get; }

        [JsonProperty("explicit_content_filter")]
        public ExplicitContentFilterLevel ExplicitContentFilterLevel { get; }

        [JsonProperty("roles")]
        public Role[] Roles { get; }

        [JsonProperty("channels")]
        public PartialChannel[] Channels { get; }

        public GuildCreationArguments(string name, string voiceRegionId, string iconBase64, VerificationLevel verificationLevel = VerificationLevel.None, 
            MessageNotificationLevel messageNotificationLevel = MessageNotificationLevel.OnlyMentions, ExplicitContentFilterLevel explicitContentFilterLevel = ExplicitContentFilterLevel.Disabled, 
            Role[] roles = null, PartialChannel[] channels = null)
        {
            Name = name;
            VoiceRegionId = voiceRegionId;
            IconBase64 = iconBase64;
            VerificationLevel = verificationLevel;
            MessageNotificationLevel = messageNotificationLevel;
            ExplicitContentFilterLevel = explicitContentFilterLevel;
            Roles = roles;
            Channels = channels;
        }
    }
}