using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fractum.WebSocket;

namespace Fractum.WebSocket
{
    public sealed class CachedGuild : PopulatedEntity
    {
        internal CachedGuild(FractumCache cache, ulong id) : base(cache)
        {
            Id = id;
        }

        private SyncedGuildCache GuildCache => Cache.TryGetGuild(Id, out var guild) ? guild : default;

        public ulong OwnerId => GuildCache.OwnerId;

        public bool IsUnavailable => GuildCache.IsUnavailable;

        public string Region => GuildCache.Region;

        public string Name => GuildCache.Name;

        public int MemberCount => GuildCache.MemberCount;

        public bool Lazy => GuildCache.Lazy;

        public bool Large => GuildCache.Large;

        public int AfkTimeout => GuildCache.AfkTimeout;

        public VerificationLevel VerificationLevel => GuildCache.VerificationLevel;

        public MessageNotificationLevel MessageNotificationLevel => GuildCache.MessageNotificationLevel;

        public ExplicitContentFilterLevel ExplicitContentFilterLevel => GuildCache.ExplicitContentFilterLevel;

        public bool RequireMfa => GuildCache.RequireMfa;

        internal ulong? AfkChannelId => GuildCache.AfkChannelId;

        internal string IconHash => GuildCache.IconHash;

        internal string SplashHash => GuildCache.SplashHash;

        public IEnumerable<GuildEmoji> Emojis => GuildCache.Emojis;

        public IEnumerable<Role> Roles => GuildCache.Roles;

        public IEnumerable<CachedMember> Members => GuildCache.Members;

        public CachedMember Owner => GuildCache.TryGet(OwnerId, out CachedMember member) ? member : default;

        public IEnumerable<CachedGuildChannel> Channels => GuildCache.Channels;

        public IEnumerable<CachedTextChannel> TextChannels => Channels
            .Where(c => c.Type == ChannelType.GuildText)
            .Cast<CachedTextChannel>();

        public IEnumerable<CachedVoiceChannel> VoiceChannels => Channels
            .Where(c => c.Type == ChannelType.GuildVoice)
            .Cast<CachedVoiceChannel>();

        public IEnumerable<CachedCategory> Categories => Channels
            .Where(c => c.Type == ChannelType.GuildCategory)
            .Cast<CachedCategory>();

        public Task LeaveAsync()
            => Client.RestClient.LeaveGuildAsync(Id);

        public string GetIconUrl() => IconHash == null
            ? default
            : string.Concat(Consts.CDN, string.Format(Consts.CDN_GUILD_ICON, Id.ToString(), IconHash, "png"));

        public string GetSplashUrl() => SplashHash == null
            ? default
            : string.Concat(Consts.CDN, string.Format(Consts.CDN_GUILD_SPLASH, Id.ToString(), SplashHash, ".png"));

        public override string ToString()
            => $"{Name} : {Id.ToString()}";
    }
}