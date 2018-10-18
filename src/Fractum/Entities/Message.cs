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
    public sealed class Message : DiscordEntity
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
        public ulong ChannelId { get; private set; }

        [JsonProperty("guild_id")]
        public ulong? GuildId { get; private set; }

        [JsonProperty("member")]
        private PartialMember Member { get; set; } // TODO: See what to do with this.

        [JsonProperty("author")]
        private User AuthorUser { get; set; }

        [JsonIgnore]
        public IUser Author =>  Guild.GetMembers().FirstOrDefault(x => x.Id == AuthorUser.Id) as IUser ?? AuthorUser;

        [JsonIgnore]
        public IMessageChannel Channel => Guild.GetChannels().First(x => x.Id == ChannelId) as IMessageChannel;

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