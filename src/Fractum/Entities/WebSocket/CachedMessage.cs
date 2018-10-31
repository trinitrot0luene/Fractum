using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fractum.Entities.Rest;
using Fractum.WebSocket;
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

            cache.AddOrReplace(model.AuthorUser);
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

        public IUser Author
        {
            get
            {
                if (Cache.TryGetGuild(ChannelId, out var guild, SearchType.Channel))
                {
                    if (guild.TryGet(AuthorId, out CachedMember member))
                        return member as IUser;
                }

                return Cache.TryGetUser(AuthorId, out var user) ? user : default;
            }
        }

        public IEnumerable<Role> MentionedRoles
        {
            get
            {
                if (Cache.TryGetGuild(ChannelId, out var guild, SearchType.Channel))
                {
                    foreach (var rid in MentionedRoleIds)
                    {
                        if (guild.TryGet(rid, out Role role))
                            yield return role;
                    }
                }
            }
        }

        public IMessageChannel Channel
        {
            get
            {
                if (Cache.TryGetGuild(ChannelId, out var guild, SearchType.Channel)
                    && guild.TryGet(ChannelId, out CachedGuildChannel channel))
                    return channel as IMessageChannel;
                else return Cache.TryGetDmChannel(ChannelId, out var dmChannel) ? dmChannel : default;
            }
        }

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
            => Client.RestClient.CreateReactionAsync(this.Id, this.ChannelId, emoji);

        public Task DeleteReactionAsync(Emoji emoji, IUser user = null)
            => Client.RestClient.DeleteReactionAsync(this.Id, this.ChannelId, emoji, user != null ? user.Id : default);

        public Task ClearReactionsAsync()
            => Client.RestClient.ClearReactionsAsync(this.Id, this.ChannelId);

        public Task<IReadOnlyCollection<User>> GetReactionsAsync(Emoji emoji, int limit = 25)
            => Client.RestClient.GetReactionsAsync(this.Id, this.ChannelId, emoji, limit);

        public Task<RestMessage> EditAsync(Action<MessageEditProperties> updateAction)
        {
            var props = new MessageEditProperties();
            updateAction(props);

            return Client.RestClient.EditMessageAsync(this.Id, this.ChannelId, props);
        }

        public Task DeleteAsync()
            => Client.RestClient.DeleteMessageAsync(this.Id, this.ChannelId);

        #endregion
    }
}