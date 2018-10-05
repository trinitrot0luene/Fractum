using System;
using System.Collections.ObjectModel;
using System.Linq;
using Fractum.Entities.Contracts;
using Fractum.WebSocket.Entities;
using Newtonsoft.Json;

namespace Fractum.Entities
{
    public sealed class Message : DiscordEntity
    {
        [JsonProperty("tts")]
        public bool IsTTS { get; private set; }

        [JsonProperty("type")]
        public MessageType Type { get; private set; }

        [JsonIgnore]
        public bool IsUserMessage => Type == MessageType.Default;

        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; private set; }

        [JsonProperty("pinned")]
        public bool IsPinned { get; private set; }

        [JsonProperty("mentions")]
        public ReadOnlyCollection<User> MentionedUsers { get; private set; }

        [JsonProperty("mention_roles")]
        internal ulong[] MentionedRoleIds { get; private set; }

        [JsonProperty("mention_everyone")]
        public bool IsEveryoneMention { get; private set; }

        [JsonProperty("embeds")]
        public ReadOnlyCollection<Embed> Embeds { get; private set; }

        [JsonProperty("edited_timestamp")]
        public DateTimeOffset? LastEditedAt { get; private set; }

        [JsonProperty("content")]
        public string Content { get; private set; }

        [JsonProperty("channel_id")]
        public ulong ChannelId { get; private set; }

        [JsonProperty("guild_id")]
        public ulong? GuildId { get; private set; }

        [JsonProperty("member")]
        internal PartialMember Member { get; private set; }

        [JsonProperty("author")]
        private User AuthorUser { get; set; }

        [JsonIgnore]
        public IUser Author
        {
            get
            {
                var member = Guild.Members.FirstOrDefault(m => m.Id == AuthorUser.Id);
                if (member is null)
                    return AuthorUser;
                return member;
            }
        }

        [JsonIgnore]
        public TextChannel Channel => Guild.TextChannels.FirstOrDefault(c => c.Id == ChannelId);

        [JsonIgnore]
        public Guild Guild { get; internal set; }
    }
}