using System.Collections.ObjectModel;
using System.Linq;
using Fractum.Entities.WebSocket;
using Fractum.WebSocket.EventModels;

namespace Fractum.Entities
{
    public sealed class Guild : DiscordEntity
    {
        internal Guild(GuildCreateEventModel model)
        {
            Id = model.Id;
            OwnerId = model.OwnerId;
            IsUnavailable = model.IsUnavailable;
            Region = model.Region;
            Name = model.Name;
            MemberCount = model.MemberCount;
            Lazy = model.Lazy;
            Large = model.Large;
            AfkTimeout = model.AfkTimeout;
            AfkChannelId = model.AfkChannelId;
            VerificationLevel = (VerificationLevel) model.VerificationLevel;
            MessageNotificationLevel = (MessageNotificationLevel) model.DefaultMessageNotifications;
            ExplicitContentFilterLevel = (ExplicitContentFilterLevel) model.ExplicitContentFilter;
            RequireMfa = model.RequireMfa;
        }

        private Guild()
        {
        }

        public ulong OwnerId { get; internal set; }

        public bool IsUnavailable { get; internal set; }

        public string Region { get; internal set; }

        public string Name { get; internal set; }

        public int MemberCount { get; internal set; }

        public bool Lazy { get; internal set; }

        public bool Large { get; internal set; }

        public int AfkTimeout { get; internal set; }

        public VerificationLevel VerificationLevel { get; internal set; }

        public MessageNotificationLevel MessageNotificationLevel { get; internal set; }

        public ExplicitContentFilterLevel ExplicitContentFilterLevel { get; internal set; }

        public bool RequireMfa { get; internal set; }

        internal ulong? AfkChannelId { get; set; }

        internal string IconHash { get; set; }

        internal string SplashHash { get; set; }

        public ReadOnlyCollection<Emoji> Emoji { get; internal set; }

        public ReadOnlyCollection<Role> Roles { get; internal set; }

        public ReadOnlyCollection<GuildMember> Members { get; internal set; }

        public ReadOnlyCollection<Message> Messages { get; internal set; }

        public GuildMember Owner => Members.FirstOrDefault(m => m.Id == OwnerId);

        public ReadOnlyCollection<GuildChannel> Channels { get; internal set; }

        public ReadOnlyCollection<TextChannel> TextChannels => Channels
            .Where(c => c.Type == ChannelType.GuildText)
            .Cast<TextChannel>()
            .ToList()
            .AsReadOnly();

        public ReadOnlyCollection<VoiceChannel> VoiceChannels => Channels
            .Where(c => c.Type == ChannelType.GuildVoice)
            .Cast<VoiceChannel>()
            .ToList()
            .AsReadOnly();

        public ReadOnlyCollection<Category> Categories => Channels
            .Where(c => c.Type == ChannelType.GuildCategory)
            .Cast<Category>()
            .ToList()
            .AsReadOnly();

        public ReadOnlyCollection<Presence> Presences { get; internal set; }

        public string GetIconUrl() => IconHash == null
            ? default
            : string.Concat(Consts.CDN, string.Format(Consts.CDN_GUILD_ICON, Id, IconHash, "png"));

        public string GetSplashUrl() => SplashHash == null
            ? default
            : string.Concat(Consts.CDN, string.Format(Consts.CDN_GUILD_SPLASH, Id, SplashHash, ".png"));
    }
}