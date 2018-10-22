using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities.Properties;
using Fractum.Entities.Rest;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;

namespace Fractum.Entities.WebSocket
{
    public sealed class CachedMessage : PopulatedEntity, IMessage, ICloneable
    {
        internal CachedMessage(FractumCache cache, MessageCreateEventModel model) : base(cache)
        {
            Id = model.Id;

            AuthorId = model.AuthorUser.Id;
            IsTTS = model.IsTTS;
            Type = model.Type;
            Timestamp = model.Timestamp;
            IsPinned = model.IsPinned;
            MentionedUsers = model.MentionedUsers;
            Attachments = model.Attachments;
            MentionedRoleIds = model.MentionedRoleIds;
            MentionsEveryone = model.MentionsEveryone;
            Embeds = model.Embeds;
            LastEditedAt = model.LastEditedAt;
            Content = model.Content;
            ChannelId = model.ChannelId;
            GuildId = model.GuildId;

            cache.AddUser(model.AuthorUser);
        }

        private CachedMessage(FractumCache cache) : base(cache)
        {
        }

        public object Clone() => new CachedMessage(Cache)
        {
            AuthorId = AuthorId,
            IsTTS = IsTTS,
            Type = Type,
            Timestamp = Timestamp,
            IsPinned = IsPinned,
            MentionedUsers = MentionedUsers,
            Attachments = Attachments,
            MentionedRoleIds = MentionedRoleIds,
            MentionsEveryone = MentionsEveryone,
            Embeds = Embeds,
            LastEditedAt = LastEditedAt,
            Content = Content,
            ChannelId = ChannelId,
            GuildId = GuildId
        };

        internal void Update(MessageUpdateEventModel model)
        {
            IsTTS = model.IsTTS;
            IsPinned = model.IsPinned;
            MentionedUsers = model.MentionedUsers;
            MentionedRoleIds = model.MentionedRoleIds;
            MentionsEveryone = model.MentionsEveryone;
            Embeds = model.Embeds;
            LastEditedAt = model.LastEditedAt;
            Content = model.Content;
        }

        public override string ToString()
            => $"{Id} : {Content}";

        #region Populated Properties

        public IUser Author => Cache[this].GetMember(AuthorId) as IUser ?? Cache.GetUserOrDefault(AuthorId);

        public IEnumerable<Role> MentionedRoles
        {
            get
            {
                var guild = Cache[this];
                if (guild != null)
                    foreach (var rid in MentionedRoleIds)
                    {
                        var role = guild.GetRole(rid);
                        if (role != null)
                            yield return role;
                    }
            }
        }

        public IMessageChannel Channel => Cache[this].GetChannel(ChannelId) as IMessageChannel;

        #endregion

        #region Cached Properties

        public ulong AuthorId { get; private set; }

        public bool IsTTS { get; private set; }

        public MessageType Type { get; private set; }

        public bool IsUserMessage => Type == MessageType.Default;

        public DateTimeOffset Timestamp { get; private set; }

        public bool IsPinned { get; private set; }

        public IEnumerable<User> MentionedUsers { get; private set; }

        public IEnumerable<Attachment> Attachments { get; private set; }

        public ulong[] MentionedRoleIds { get; private set; }

        public bool MentionsEveryone { get; internal set; }

        public IEnumerable<Embed> Embeds { get; private set; }

        public DateTimeOffset? LastEditedAt { get; private set; }

        public string Content { get; private set; }

        public ulong ChannelId { get; private set; }

        public ulong? GuildId { get; private set; }

        #endregion

        #region REST

        public Task CreateReactionAsync(Emoji emoji)
            => Client.RestClient.CreateReactionAsync(this, emoji);

        public Task DeleteReactionAsync(Emoji emoji, IUser user = null)
            => Client.RestClient.DeleteReactionAsync(this, emoji, user != null ? user.Id : default);

        public Task ClearReactionsAsync()
            => Client.RestClient.ClearReactionsAsync(this);

        public Task<IReadOnlyCollection<User>> GetReactionsAsync(Emoji emoji, int limit = 25)
            => Client.RestClient.GetReactionsAsync(this, emoji, limit);

        public Task<RestMessage> EditAsync(Action<MessageEditProperties> updateAction)
        {
            var props = new MessageEditProperties();
            updateAction(props);

            return Client.RestClient.EditMessageAsync(this, props);
        }

        public Task DeleteAsync()
            => Client.RestClient.DeleteMessageAsync(this);

        #endregion
    }
}