using System;
using System.Collections.Generic;
using System.Linq;
using Fractum.Entities;
using Fractum.Entities.WebSocket;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket
{
    public sealed class SyncedGuildCache
    {
        private readonly object channelLock = new object();
        private readonly object emojiLock = new object();
        private readonly object memberLock = new object();
        private readonly object messageLock = new object();
        private readonly object roleLock = new object();

        internal FractumCache Cache;
        internal FractumSocketClient Client;

        public CachedGuild Guild { get; }

        private Dictionary<ulong, Role> roles = new Dictionary<ulong, Role>();
        private Dictionary<ulong, GuildEmoji> emojis = new Dictionary<ulong, GuildEmoji>();
        private readonly Dictionary<ulong, CachedMember> members = new Dictionary<ulong, CachedMember>();
        private readonly Dictionary<ulong, CachedGuildChannel> channels = new Dictionary<ulong, CachedGuildChannel>();
        private readonly Dictionary<ulong, CircularBuffer<CachedMessage>> messages = new Dictionary<ulong, CircularBuffer<CachedMessage>>();

        internal SyncedGuildCache(FractumCache cache, GuildCreateEventModel model)
        {
            Client = cache.Client;
            Cache = cache;

            Id = model.Id;
            OwnerId = model.OwnerId;
            IsUnavailable = model.IsUnavailable;
            Region = model.Region;
            Name = model.Name;
            MemberCount = model.MemberCount;
            Lazy = model.Lazy;
            Large = model.Large;
            AfkTimeout = model.AfkTimeout;
            AfkChannelId = model.AfkChannelId;
            VerificationLevel = (VerificationLevel) model.VerificationLevel;
            MessageNotificationLevel = (MessageNotificationLevel) model.DefaultMessageNotifications;
            ExplicitContentFilterLevel = (ExplicitContentFilterLevel) model.ExplicitContentFilter;
            RequireMfa = model.RequireMfa;

            lock (emojiLock)
            {
                foreach (var emoji in model.Emojis)
                    AddOrReplace(new GuildEmoji(Cache, emoji));
            }

            lock (roleLock)
            {
                foreach (var role in model.Roles)
                    AddOrReplace(role);
            }

            lock (channelLock)
            {
                foreach (var channel in model.Channels)
                    switch (channel.Type)
                    {
                        case ChannelType.GuildCategory:
                            AddOrReplace(new CachedCategory(Cache, channel, Id));
                            break;
                        case ChannelType.GuildText:
                            AddOrReplace(new CachedTextChannel(Cache, channel, Id));
                            break;
                        case ChannelType.GuildVoice:
                            AddOrReplace(new CachedVoiceChannel(Cache, channel, Id));
                            break;
                    }
            }

            lock (memberLock)
            {
                foreach (var member in model.Members)
                {
                    AddOrReplace(new CachedMember(Cache, member, Id));

                    Cache.AddOrReplace(member.User);
                }
            }

            foreach (var presence in model.Presences)
                Cache.AddOrReplace(new CachedPresence(presence));

            Guild = new CachedGuild(Cache, Id);
        }

        #region Properties

        public ulong Id { get; }

        public ulong OwnerId { get; internal set; }

        public bool IsUnavailable { get; set; }

        public string Region { get; internal set; }

        public string Name { get; internal set; }

        public int MemberCount { get; internal set; }

        public bool Lazy { get; internal set; }

        public bool Large { get; internal set; }

        public int AfkTimeout { get; internal set; }

        public VerificationLevel VerificationLevel { get; internal set; }

        public MessageNotificationLevel MessageNotificationLevel { get; internal set; }

        public ExplicitContentFilterLevel ExplicitContentFilterLevel { get; internal set; }

        public bool RequireMfa { get; internal set; }

        public ulong? AfkChannelId { get; set; }

        public string IconHash { get; set; }

        public string SplashHash { get; set; }

        public IEnumerable<CachedMember> Members
        {
            get
            {
                lock (memberLock)
                    foreach (var memberPair in members)
                        yield return memberPair.Value;
            }
        }

        public IEnumerable<CachedGuildChannel> Channels
        {
            get
            {
                lock (channelLock)
                    foreach (var channelPair in channels)
                        yield return channelPair.Value;
            }
        }

        public IEnumerable<Role> Roles
        {
            get
            {
                lock (roleLock)
                    foreach (var rolePair in roles)
                        yield return rolePair.Value;
            }
        }

        public IEnumerable<GuildEmoji> Emojis
        {
            get
            {
                lock (emojiLock)
                    foreach (var emojiPair in emojis)
                        yield return emojiPair.Value;
            }
        }

        #endregion

        #region Add | Update | Create

        public void Update(GuildUpdateEventModel eventModel)
        {
            OwnerId = eventModel.OwnerId;
            IsUnavailable = eventModel.IsUnavailable;
            Region = eventModel.Region;
            Name = eventModel.Name;
            MemberCount = eventModel.MemberCount;
            Lazy = eventModel.Lazy;
            Large = eventModel.Large;
            AfkTimeout = eventModel.AfkTimeout;
            AfkChannelId = eventModel.AfkChannelId ?? this.AfkChannelId;
            VerificationLevel = (VerificationLevel)eventModel.VerificationLevel;
            MessageNotificationLevel = (MessageNotificationLevel)eventModel.DefaultMessageNotifications;
            ExplicitContentFilterLevel = (ExplicitContentFilterLevel)eventModel.ExplicitContentFilter;
            RequireMfa = eventModel.RequireMfa;

            AddOrReplace(eventModel.Roles);
            AddOrReplace(eventModel.Emojis);
        }

        public bool TryGet(ulong memberId, out CachedMember member)
        {
            lock (memberLock)
                return members.TryGetValue(memberId, out member);
        }

        public bool TryGet(ulong channelId, out CachedGuildChannel channel)
        {
            lock (channelLock)
                return channels.TryGetValue(channelId, out channel);
        }

        internal bool TryGet(ulong channelId, out CircularBuffer<CachedMessage> cachedMessages)
        {
            lock (messageLock)
                return messages.TryGetValue(channelId, out cachedMessages);
        }

        public bool TryGet(ulong roleId, out Role role)
        {
            lock (roleLock)
                return roles.TryGetValue(roleId, out role);
        }

        public bool TryGet(ulong emojiId, out GuildEmoji emoji)
        {
            lock (emojiLock)
                return emojis.TryGetValue(emojiId, out emoji);
        }

        public void AddOrReplace(CachedMember member)
        {
            lock (memberLock)
                members[member.Id] = member;
        }

        public void AddOrReplace(CachedGuildChannel channel)
        {
            lock (channelLock)
                channels[channel.Id] = channel;
        }

        public void AddOrReplace(Role role)
        {
            lock (roleLock)
                roles[role.Id] = role;
        }

        public void AddOrReplace(IEnumerable<Role> newRoles)
        {
            lock (roleLock)
            {
                roles = new Dictionary<ulong, Role>();
                foreach (var role in newRoles)
                    roles.Add(role.Id, role);
            }
        }

        public void AddOrReplace(GuildEmoji emoji)
        {
            lock (emojis)
                emojis[emoji.Id] = emoji;
        }

        public void AddOrReplace(IEnumerable<GuildEmoji> newEmojis)
        {
            lock (emojiLock)
            {
                emojis = new Dictionary<ulong, GuildEmoji>();
                foreach (var emoji in newEmojis)
                    emojis.Add(emoji.Id, emoji);
            }
        }

        public void AddOrReplace(CachedMessage message)
        {
            lock (messageLock)
            {
                if (TryGet(message.ChannelId, out CircularBuffer<CachedMessage> rb))
                {
                    rb = rb ?? new CircularBuffer<CachedMessage>(Client.RestClient.Config.MessageCacheLength);

                    if (rb.FirstOrDefault(m => m.Id == message.Id) is CachedMessage oldMessage)
                        rb[rb.IndexOf(oldMessage)] = message;
                    else
                        rb.Add(message);

                }
            }
        }

        public bool RemoveMember(ulong memberId)
        {
            lock (memberLock)
                return members.Remove(memberId);
        }

        public bool RemoveChannel(ulong channelId)
        {
            lock (channelLock)
                return channels.Remove(channelId);
        }

        public bool RemoveRole(ulong roleId)
        {
            lock (roleLock)
                return roles.Remove(roleId);
        }

        public bool RemoveEmoji(ulong emojiId)
        {
            lock (emojiLock)
                return emojis.Remove(emojiId);
        }

        #endregion
    }
}