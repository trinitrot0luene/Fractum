using System.Collections.Generic;
using Fractum.Entities;
using Fractum.Entities.WebSocket;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket
{
    public interface ISyncedGuild
    {
        CachedGuild Guild { get; }

        ulong Id { get; }

        ulong OwnerId { get; }

        bool IsUnavailable { get; set; }

        string Region { get; }

        string Name { get; }

        int MemberCount { get; }

        bool Lazy { get; }

        bool Large { get; }

        int AfkTimeout { get; }

        ulong? AfkChannelId { get; }

        string IconHash { get; }

        string SplashHash { get; }

        VerificationLevel VerificationLevel { get; }

        MessageNotificationLevel MessageNotificationLevel { get; }

        ExplicitContentFilterLevel ExplicitContentFilterLevel { get; }

        bool RequireMfa { get; }

        IEnumerable<CachedMember> Members { get; }

        IEnumerable<CachedGuildChannel> Channels { get; }

        IEnumerable<Role> Roles { get; }

        IEnumerable<GuildEmoji> Emojis { get; }

        void Update(GuildUpdateEventModel model);

        bool TryGet(ulong memberId, out CachedMember member);

        bool TryGet(ulong channelId, out CachedGuildChannel channel);

        bool TryGet(ulong channelId, out CircularBuffer<CachedMessage> messages);

        bool TryGet(ulong roleId, out Role role);

        bool TryGet(ulong emojiId, out GuildEmoji emoji);

        void AddOrReplace(CachedMember member);

        void AddOrReplace(CachedGuildChannel channel);

        void AddOrReplace(Role role);

        void AddOrReplace(IEnumerable<Role> roles);

        void AddOrReplace(GuildEmoji emoji);

        void AddOrReplace(IEnumerable<GuildEmoji> emojis);

        void AddOrReplace(CachedMessage message);

        bool RemoveMember(ulong memberId);

        bool RemoveChannel(ulong channelId);

        bool RemoveRole(ulong roleId);

        bool RemoveEmoji(ulong emojiId);
    }
}