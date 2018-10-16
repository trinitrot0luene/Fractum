using System.Collections.ObjectModel;
using System.Linq;
using Fractum.Entities.WebSocket;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;

namespace Fractum.Entities
{
    public sealed class Guild : DiscordEntity
    {
        private readonly GuildCache _cache;

        internal Guild(GuildCache cache)
        {
            _cache = cache;

            Id = _cache.Id;

            WithClient(_cache.Client);
        }

        public ulong OwnerId => _cache.OwnerId;

        public bool IsUnavailable => _cache.IsUnavailable;

        public string Region => _cache.Region;

        public string Name => _cache.Name;

        public int MemberCount => _cache.MemberCount;

        public bool Lazy => _cache.Lazy;

        public bool Large => _cache.Large;

        public int AfkTimeout => _cache.AfkTimeout;

        public VerificationLevel VerificationLevel => _cache.VerificationLevel;

        public MessageNotificationLevel MessageNotificationLevel => _cache.MessageNotificationLevel;

        public ExplicitContentFilterLevel ExplicitContentFilterLevel => _cache.ExplicitContentFilterLevel;

        public bool RequireMfa => _cache.RequireMfa;

        internal ulong? AfkChannelId => _cache.AfkChannelId;

        internal string IconHash => _cache.IconHash;

        internal string SplashHash => _cache.SplashHash;

        public ReadOnlyCollection<GuildEmoji> Emojis => _cache.GetEmojis();

        public ReadOnlyCollection<Role> Roles => _cache.GetRoles();

        public ReadOnlyCollection<GuildMember> Members => _cache.GetMembers();

        public GuildMember Owner => Members.FirstOrDefault(m => m.Id == OwnerId);

        public ReadOnlyCollection<GuildChannel> Channels => _cache.GetChannels();

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

        public ReadOnlyCollection<Presence> Presences => _cache.GetPresences();

        public string GetIconUrl() => IconHash == null
            ? default
            : string.Concat(Consts.CDN, string.Format(Consts.CDN_GUILD_ICON, Id, IconHash, "png"));

        public string GetSplashUrl() => SplashHash == null
            ? default
            : string.Concat(Consts.CDN, string.Format(Consts.CDN_GUILD_SPLASH, Id, SplashHash, ".png"));

        public override string ToString()
            => $"{Name} : {Id}";
    }
}