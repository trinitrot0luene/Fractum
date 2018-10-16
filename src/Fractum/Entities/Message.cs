using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities.Properties;
using Fractum.Entities.WebSocket;
using Fractum.WebSocket;
using Newtonsoft.Json;

namespace Fractum.Entities
{
    public sealed class Message : DiscordEntity
    {
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

        [JsonIgnore]
        public ReadOnlyCollection<Role> MentionedRoles { get; internal set; }

        [JsonProperty("mention_roles")]
        internal ulong[] MentionedRoleIds { get; set; }

        [JsonProperty("mention_everyone")]
        public bool IsEveryoneMention { get; internal set; }

        [JsonProperty("embeds")]
        public ReadOnlyCollection<Embed> Embeds { get; internal set; }

        [JsonProperty("edited_timestamp")]
        public DateTimeOffset? LastEditedAt { get; internal set; }

        [JsonProperty("content")]
        public string Content { get; internal set; }

        [JsonProperty("channel_id")]
        public ulong ChannelId { get; internal set; }

        [JsonProperty("guild_id")]
        public ulong? GuildId { get; internal set; }

        [JsonProperty("member")]
        internal PartialMember Member { get; set; }

        [JsonProperty("author")]
        internal User AuthorUser { get; set; }

        [JsonIgnore]
        public IUser Author
        {
            get
            {
                if (Channel is PrivateChannel)
                    return AuthorUser;
                return (Channel as TextChannel).Guild.Members.FirstOrDefault(m => m.Id == AuthorUser.Id) ?? Author;
            }
        }

        [JsonIgnore]
        public IMessageChannel Channel { get; internal set; }

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