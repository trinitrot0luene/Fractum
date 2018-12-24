using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fractum.Rest;

namespace Fractum
{
    public interface IMessage : IDiscordEntity
    {
        ulong AuthorId { get; }

        bool IsTTS { get; }

        MessageType Type { get; }

        bool IsUserMessage { get; }

        DateTimeOffset Timestamp { get; }

        bool IsPinned { get; }

        IEnumerable<IUser> MentionedUsers { get; }

        IEnumerable<Attachment> Attachments { get; }

        ulong[] MentionedRoleIds { get; }

        bool MentionsEveryone { get; }

        IEnumerable<Embed> Embeds { get; }

        DateTimeOffset? LastEditedAt { get; }

        string Content { get; }

        ulong ChannelId { get; }

        ulong? GuildId { get; }

        Task CreateReactionAsync(Emoji emoji);

        Task DeleteReactionAsync(Emoji emoji, IUser user = null);

        Task ClearReactionsAsync();

        Task<IReadOnlyCollection<RestUser>> GetReactionsAsync(Emoji emoji, int limit = 25);

        Task<RestMessage> EditAsync(Action<MessageEditProperties> updateAction);
    }
}