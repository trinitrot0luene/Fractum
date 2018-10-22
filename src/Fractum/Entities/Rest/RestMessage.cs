using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Fractum.Contracts;
using Newtonsoft.Json;

namespace Fractum.Entities.Rest
{
    public class RestMessage : DiscordEntity, IMessage
    {
        [JsonProperty("mentions")]
        public ReadOnlyCollection<User> MentionedUsers { get; private set; }

        [JsonProperty("attachments")]
        public ReadOnlyCollection<Attachment> Attachments { get; private set; }

        [JsonProperty("embeds")]
        public ReadOnlyCollection<Embed> Embeds { get; private set; }

        [JsonProperty("author")]
        public User Author { get; private set; }

        public ulong AuthorId => Author.Id;

        public bool IsUserMessage => Type == MessageType.Default;

        [JsonProperty("tts")]
        public bool IsTTS { get; private set; }

        [JsonProperty("type")]
        public MessageType Type { get; private set; }

        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; private set; }

        [JsonProperty("pinned")]
        public bool IsPinned { get; private set; }

        IEnumerable<User> IMessage.MentionedUsers => MentionedUsers;

        IEnumerable<Attachment> IMessage.Attachments => Attachments;

        IEnumerable<Embed> IMessage.Embeds => Embeds;

        [JsonProperty("mention_roles")]
        public ulong[] MentionedRoleIds { get; private set; }

        [JsonProperty("mention_everyone")]
        public bool MentionsEveryone { get; private set; }

        [JsonProperty("edited_timestamp")]
        public DateTimeOffset? LastEditedAt { get; private set; }

        [JsonProperty("content")]
        public string Content { get; private set; }

        [JsonProperty("channel_id")]
        public ulong ChannelId { get; private set; }

        [JsonProperty("guild_id")]
        public ulong? GuildId { get; private set; }
    }
}