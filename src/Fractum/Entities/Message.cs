using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities.Properties;
using Fractum.Entities.WebSocket;
using Fractum.WebSocket;
using Fractum.WebSocket.Core;
using Newtonsoft.Json;

namespace Fractum.Entities
{
    public sealed class Message : DiscordEntity, ICloneable
    {
        [JsonIgnore]
        internal GuildCache Guild;

        [JsonProperty("tts")]
        public bool IsTTS { get; internal set; }

        [JsonProperty("type")]
        public MessageType Type { get; internal set; }

        [JsonIgnore]
        public bool IsUserMessage => Type == MessageType.Default;

        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; internal set; }

        [JsonProperty("pinned")]
        public bool IsPinned { get; internal set; }

        [JsonProperty("mentions")]
        public ReadOnlyCollection<User> MentionedUsers { get; internal set; }

        [JsonProperty("attachments")]
        public ReadOnlyCollection<Attachment> Attachments { get; internal set; }

        [JsonProperty("mention_roles")]
        private ulong[] MentionedRoleIds { get; set; }

        [JsonProperty("mention_everyone")]
        public bool IsEveryoneMention { get; internal set; }

        [JsonProperty("embeds")]
        public ReadOnlyCollection<Embed> Embeds { get; private set; }

        [JsonProperty("edited_timestamp")]
        public DateTimeOffset? LastEditedAt { get; private set; }

        [JsonProperty("content")]
        public string Content { get; private set; }

        [JsonProperty("channel_id")]
        internal ulong ChannelId { get; private set; }

        [JsonProperty("guild_id")]
        internal ulong? GuildId { get; private set; }

        // [JsonProperty("member")]
        // private PartialMember Member { get; set; } TODO: See what to do with this.

        [JsonProperty("author")]
        private User AuthorUser { get; set; }

        [JsonIgnore]
        public IUser Author =>  Guild.GetMember(AuthorUser.Id) as IUser ?? AuthorUser;

        [JsonIgnore]
        public IMessageChannel Channel => Guild.GetChannel(ChannelId) as IMessageChannel;

        public object Clone()
        {
            return new Message()
            {
                Id = this.Id,
                ChannelId = this.ChannelId,
                GuildId = this.GuildId,
                Timestamp = this.Timestamp,
                Guild = this.Guild,
                AuthorUser = this.AuthorUser,
                Content = this.Content,
                LastEditedAt = this.LastEditedAt,
                Embeds = this.Embeds,
                Attachments = this.Attachments,
                IsEveryoneMention = this.IsEveryoneMention,
                MentionedRoleIds = this.MentionedRoleIds,
                MentionedUsers = this.MentionedUsers, 
                IsPinned = this.IsPinned,
                IsTTS = this .IsTTS,
                Type = this.Type
            };
        }

        internal void Update(Message message)
        {
            MentionedUsers = message.MentionedUsers ?? MentionedUsers;
            Embeds = message.Embeds ?? Embeds;
            IsTTS = message.IsTTS;
            Timestamp = message.Timestamp;
            IsPinned = message.IsPinned;
            IsEveryoneMention = message.IsEveryoneMention;
            LastEditedAt = message.LastEditedAt;
            Content = message.Content ?? Content;
            Attachments = message.Attachments ?? Attachments;

            MentionedRoleIds = message.MentionedRoleIds ?? MentionedRoleIds;
        }

        [JsonIgnore]
        public ReadOnlyCollection<Role> MentionedRoles =>
            Guild.GetRoles().Where(x => MentionedRoleIds?.Any(rid => rid == x.Id) ?? false).ToList().AsReadOnly();

        public Task CreateReactionAsync(Emoji emoji)
            => Client.RestClient.CreateReactionAsync(this, emoji);

        public Task DeleteReactionAsync(Emoji emoji, IUser user = null)
            => Client.RestClient.DeleteReactionAsync(this, emoji, user != null ? user.Id : default);

        public Task ClearReactionsAsync()
            => Client.RestClient.ClearReactionsAsync(this);

        public Task<IReadOnlyCollection<User>> GetReactionsAsync(Emoji emoji, int limit = 25)
            => Client.RestClient.GetReactionsAsync(this, emoji, limit);

        public Task<Message> EditAsync(Action<MessageEditProperties> updateAction)
        {
            var props = new MessageEditProperties();
            updateAction(props);

            return Client.EditMessageAsync(this, props);
        }

        public Task DeleteAsync()
            => Client.RestClient.DeleteMessageAsync(this);

        public override string ToString()
            => $"{Id} : {Content}";
    }
}