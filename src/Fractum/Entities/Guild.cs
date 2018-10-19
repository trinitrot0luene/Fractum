using System.Collections.Generic;
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

        public IReadOnlyCollection<GuildEmoji> Emojis => _cache.GetEmojis();

        public IReadOnlyCollection<Role> Roles => _cache.GetRoles();

        public IReadOnlyCollection<GuildMember> Members => _cache.GetMembers();

        public GuildMember Owner => _cache.GetMember(OwnerId);

        public IReadOnlyCollection<GuildChannel> Channels => _cache.GetChannels();

        public IReadOnlyCollection<TextChannel> TextChannels => Channels
            .Where(c => c.Type == ChannelType.GuildText)
            .Cast<TextChannel>() 
            as IReadOnlyCollection<TextChannel>;

        public IReadOnlyCollection<VoiceChannel> VoiceChannels => Channels
            .Where(c => c.Type == ChannelType.GuildVoice)
            .Cast<VoiceChannel>()
            as IReadOnlyCollection<VoiceChannel>;

        public IReadOnlyCollection<Category> Categories => Channels
            .Where(c => c.Type == ChannelType.GuildCategory)
            .Cast<Category>()
            as IReadOnlyCollection<Category>;

        public IReadOnlyCollection<Presence> Presences => _cache.GetPresences();

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