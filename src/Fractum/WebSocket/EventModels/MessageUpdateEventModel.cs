using System;
using System.Collections.Generic;
using Fractum.Entities;
using Fractum.Entities.WebSocket;
using Newtonsoft.Json;

namespace Fractum.WebSocket.EventModels
{
    public class MessageUpdateEventModel : EventModelBase
    {
        [JsonProperty("id")]
        public ulong Id { get; private set; }

        [JsonProperty("tts")]
        public bool IsTTS { get; private set; }

        [JsonProperty("type")]
        public MessageType Type { get; private set; }

        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; private set; }

        [JsonProperty("pinned")]
        public bool IsPinned { get; private set; }

        [JsonProperty("mentions")]
        public List<User> MentionedUsers { get; private set; }

        [JsonProperty("attachments")]
        public List<Attachment> Attachments { get; private set; }

        [JsonProperty("mention_roles")]
        public ulong[] MentionedRoleIds { get; private set; }

        [JsonProperty("mention_everyone")]
        public bool MentionsEveryone { get; private set; }

        [JsonProperty("embeds")]
        public List<Embed> Embeds { get; private set; }

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
        public User AuthorUser { get; private set; }
    }
}