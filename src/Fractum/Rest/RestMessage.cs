using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Fractum.Rest
{
    public class RestMessage : RestEntity, IMessage
    {
        internal RestMessage()
        {
        }

        [JsonProperty("mentions")]
        public ReadOnlyCollection<IUser> MentionedUsers { get; private set; }

        [JsonProperty("attachments")]
        public ReadOnlyCollection<Attachment> Attachments { get; private set; }

        [JsonProperty("embeds")]
        public ReadOnlyCollection<Embed> Embeds { get; private set; }

        [JsonProperty("author")]
        public RestUser Author { get; private set; }

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

        IEnumerable<IUser> IMessage.MentionedUsers => MentionedUsers;

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

        public Task CreateReactionAsync(Emoji emoji)
            => Client.CreateReactionAsync(this.Id, this.ChannelId, emoji);

        public Task DeleteReactionAsync(Emoji emoji, IUser user = null)
            => Client.DeleteReactionAsync(this.Id, this.ChannelId, emoji, user != null ? user.Id : default);

        public Task ClearReactionsAsync()
            => Client.ClearReactionsAsync(this.Id, this.ChannelId);

        public Task<IReadOnlyCollection<RestUser>> GetReactionsAsync(Emoji emoji, int limit = 25)
            => Client.GetReactionsAsync(this.Id, this.ChannelId, emoji, limit);

        public Task<RestMessage> EditAsync(Action<MessageEditProperties> updateAction)
        {
            var props = new MessageEditProperties();
            updateAction(props);

            return Client.EditMessageAsync(this.Id, this.ChannelId, props);
        }
    }
}