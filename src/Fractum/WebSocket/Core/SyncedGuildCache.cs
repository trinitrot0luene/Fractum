using System;
using System.Collections.Generic;
using System.Linq;
using Fractum.Entities;
using Fractum.Entities.WebSocket;
using Fractum.Utilities;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Core
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
        internal CachedGuild Guild;

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
                    emojis.Add(emoji.Id, new GuildEmoji(Cache, emoji));
            }

            lock (roleLock)
            {
                foreach (var role in model.Roles)
                    roles.Add(role.Id, role);
            }

            lock (channelLock)
            {
                foreach (var channel in model.Channels)
                    switch (channel.Type)
                    {
                        case ChannelType.GuildCategory:
                            channels.Add(channel.Id, new CachedCategory(Cache, channel, Id));
                            break;
                        case ChannelType.GuildText:
                            channels.Add(channel.Id, new CachedTextChannel(Cache, channel, Id));
                            break;
                        case ChannelType.GuildVoice:
                            channels.Add(channel.Id, new CachedVoiceChannel(Cache, channel, Id));
                            break;
                    }
            }

            lock (memberLock)
            {
                foreach (var member in model.Members)
                    if (!members.ContainsKey(member.User.Id))
                        members.Add(member.User.Id, new CachedMember(Cache, member, Id));
            }

            foreach (var presence in model.Presences)
                Cache.AddPresence(new CachedPresence(presence));

            Guild = new CachedGuild(Cache, Id);
        }

        #region Properties

        public ulong Id { get; }

        public ulong OwnerId { get; internal set; }

        public bool IsUnavailable { get; internal set; }

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

        internal ulong? AfkChannelId { get; set; }

        internal string IconHash { get; set; }

        internal string SplashHash { get; set; }

        #endregion

        #region Add | Update | Create

        public void Update(Action<SyncedGuildCache> updateAction)
            => updateAction(this);

        public void AddOrUpdate(GuildEmoji emoji, Func<GuildEmoji, GuildEmoji> replaceAction)
        {
            lock (emojiLock)
            {
                if (emojis.ContainsKey(emoji.Id))
                    emojis[emoji.Id] = replaceAction(emojis[emoji.Id]);
                else
                    emojis.Add(emoji.Id, emoji);
            }
        }

        public void AddOrUpdate(Role role, Func<Role, Role> replaceAction)
        {
            lock (roleLock)
            {
                if (roles.ContainsKey(role.Id))
                    roles[role.Id] = replaceAction(roles[role.Id]);
                else
                    roles.Add(role.Id, role);
            }
        }

        public void AddOrUpdate(CachedGuildChannel channel, Func<CachedGuildChannel, CachedGuildChannel> replaceAction)
        {
            lock (channelLock)
            {
                if (channels.ContainsKey(channel.Id))
                    channels[channel.Id] = replaceAction(channels[channel.Id]);
                else
                    channels.Add(channel.Id, channel);

                var updatedChannel = channels[channel.Id];
            }
        }

        public void Add(CachedMember member)
        {
            lock (memberLock)
            {
                if (members.ContainsKey(member.Id))
                    members[member.Id] = member;
                else
                    members.Add(member.Id, member);
            }
        }

        public void AddOrCreate(CachedMessage message)
        {
            lock (messageLock)
            {
                CircularBuffer<CachedMessage> rb;
                if (messages.ContainsKey(message.ChannelId))
                {
                    rb = messages[message.ChannelId];
                }
                else
                {
                    rb = new CircularBuffer<CachedMessage>(Client.RestClient.Config.MessageCacheLength);
                    messages.Add(message.ChannelId, rb);
                }

                if (rb.FirstOrDefault(m => m.Id == message.Id) is CachedMessage oldMessage)
                    rb[rb.IndexOf(oldMessage)] = message;
                else
                    rb.Add(message);
            }
        }

        public void Remove(GuildEmoji emoji)
        {
            lock (emojiLock)
            {
                emojis.Remove(emoji.Id);
            }
        }

        public void Remove(Role role)
        {
            lock (roleLock)
            {
                roles.Remove(role.Id);
            }
        }

        public void Remove(CachedGuildChannel channel)
        {
            lock (channelLock)
            {
                channels.Remove(channel.Id);
            }
        }

        public void Remove(CachedMember member)
        {
            lock (memberLock)
            {
                members.Remove(member.Id);
            }
        }

        public void Remove(CachedMessage message)
        {
            lock (messageLock)
            {
                if (messages.TryGetValue(message.ChannelId, out var msgBuff))
                    msgBuff.Remove(message);
            }
        }

        public void Replace(IEnumerable<Role> newCollection)
        {
            lock (roleLock)
            {
                roles = new Dictionary<ulong, Role>();

                foreach (var role in newCollection)
                    roles.Add(role.Id, role);
            }
        }

        public void Replace(IEnumerable<GuildEmoji> newCollection)
        {
            lock (emojiLock)
            {
                emojis = new Dictionary<ulong, GuildEmoji>();

                foreach (var emoji in newCollection)
                    emojis.Add(emoji.Id, emoji);
            }
        }

        public IEnumerable<GuildEmoji> GetEmojis()
        {
            lock (emojiLock)
            {
                foreach (var emoji in emojis)
                    yield return emoji.Value;
            }
        }

        public IEnumerable<Role> GetRoles()
        {
            lock (roleLock)
            {
                foreach (var role in roles)
                    yield return role.Value;
            }
        }

        public IEnumerable<CachedGuildChannel> GetChannels()
        {
            lock (channelLock)
            {
                foreach (var channel in channels)
                    yield return channel.Value;
            }
        }

        public IEnumerable<CachedMessage> GetMessages(ulong channelId)
        {
            lock (messageLock)
            {
                return messages.TryGetValue(channelId, out var messageCache)
                    ? messageCache.ToList()
                    : new List<CachedMessage>();
            }
        }

        public IEnumerable<CachedMember> GetMembers()
        {
            lock (memberLock)
            {
                foreach (var member in members)
                    yield return member.Value;
            }
        }

        public Role GetRole(ulong roleId)
        {
            lock (roleLock)
            {
                return roles.TryGetValue(roleId, out var role) ? role : default;
            }
        }

        public CachedMember GetMember(ulong userId)
        {
            lock (memberLock)
            {
                return members.TryGetValue(userId, out var member) ? member : default;
            }
        }

        public CachedGuildChannel GetChannel(ulong channelId)
        {
            lock (channelLock)
            {
                return channels.TryGetValue(channelId, out var channel) ? channel : default;
            }
        }

        #endregion
    }
}