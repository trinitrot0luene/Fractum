using System;
using System.Collections.Generic;
using Fractum.Entities;

namespace Fractum.Contracts
{
    public interface IMessage : IDiscordEntity
    {
        ulong AuthorId { get; }

        bool IsTTS { get; }

        MessageType Type { get; }

        bool IsUserMessage { get; }

        DateTimeOffset Timestamp { get; }

        bool IsPinned { get; }

        IEnumerable<User> MentionedUsers { get; }

        IEnumerable<Attachment> Attachments { get; }

        ulong[] MentionedRoleIds { get; }

        bool MentionsEveryone { get; }

        IEnumerable<Embed> Embeds { get; }

        DateTimeOffset? LastEditedAt { get; }

        string Content { get; }

        ulong ChannelId { get; }

        ulong? GuildId { get; }
    }
}