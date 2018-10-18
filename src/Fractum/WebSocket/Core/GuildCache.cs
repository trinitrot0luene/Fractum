using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.Entities.WebSocket;
using Fractum.Utilities;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Core
{
    public sealed class GuildCache
    {
        private readonly object emojiLock = new object();
        private readonly object roleLock = new object();
        private readonly object channelLock = new object();
        private readonly object memberLock = new object();
        private readonly object presenceLock = new object();
        private readonly object messageLock = new object();

        internal FractumSocketClient Client;

        private Dictionary<ulong, GuildEmoji> emojis = new Dictionary<ulong, GuildEmoji>();
        private Dictionary<ulong, Role> roles = new Dictionary<ulong, Role>();
        private Dictionary<ulong, GuildChannel> channels = new Dictionary<ulong, GuildChannel>();
        private Dictionary<ulong, GuildMember> members = new Dictionary<ulong, GuildMember>();
        private Dictionary<ulong, Presence> presences = new Dictionary<ulong, Presence>();
        private Dictionary<ulong, CircularBuffer<Message>> messages = new Dictionary<ulong, CircularBuffer<Message>>();

        public Guild Guild;

        internal GuildCache(FractumSocketClient client, GuildCreateEventModel model)
        {
            Client = client;

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

            foreach (var emoji in model.Emojis)
            {
                emoji.WithClient(client);
                AddOrUpdate(emoji, x => x = emoji);
            }

            foreach (var role in model.Roles)
            {
                role.WithClient(client);
                AddOrUpdate(role, x => x = role);
            }

            foreach (var channel in model.Channels)
            {
                channel.WithClient(client);
                channel.Cache = this;
                AddOrUpdate(channel, x => x = channel);
            }

            foreach (var member in model.Members)
            {
                member.WithClient(client);
                member.Guild = Guild;
                AddOrUpdate(member, x => x = member);
            }

            foreach (var presence in model.Presences)
            {
                AddOrUpdate(presence, x => x = presence);
            }

            Guild = new Guild(this);
        }

        #region Properties

        public ulong Id { get; internal set; }

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

        public void Update(Action<GuildCache> updateAction)
            => updateAction(this);

        public void AddOrUpdate(GuildEmoji emoji, Func<GuildEmoji, GuildEmoji> replaceAction)
        {
            lock (emojiLock)
            {
                if (emojis.ContainsKey(emoji.Id))
                    emojis[emoji.Id] = replaceAction(emojis[emoji.Id]);
                else
                    emojis.Add(emoji.Id, emoji);

                emojis[emoji.Id].WithClient(Client);
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

                roles[role.Id].WithClient(Client);
            }
        }

        public void AddOrUpdate(GuildChannel channel, Func<GuildChannel, GuildChannel> replaceAction)
        {
            lock (channelLock)
            {
                if (channels.ContainsKey(channel.Id))
                    channels[channel.Id] = replaceAction(channels[channel.Id]);
                else
                    channels.Add(channel.Id, channel);

                var updatedChannel = channels[channel.Id];
                updatedChannel.WithClient(Client);
                updatedChannel.Cache = this;
            }
        }

        public void AddOrUpdate(GuildMember member, Func<GuildMember, GuildMember> replaceAction)
        {
            lock (memberLock)
            {
                if (members.ContainsKey(member.Id))
                    members[member.Id] = replaceAction(members[member.Id]);
                else
                    members.Add(member.Id, member);

                var updatedMember = members[member.Id];
                updatedMember.WithClient(Client);
                updatedMember.Guild = Guild;
            }
        }

        public void AddOrUpdate(Presence presence, Func<Presence, Presence> replaceAction)
        {
            lock (presenceLock)
            {
                if (presences.ContainsKey(presence.User.Id))
                    presences[presence.User.Id] = replaceAction(presences[presence.User.Id]);
                else
                    presences.Add(presence.User.Id, presence);
            }
        }

        public void AddOrCreate(Message message)
        {
            lock (messageLock)
            {
                CircularBuffer<Message> rb;
                if (messages.ContainsKey(message.ChannelId))
                {
                    rb = messages[message.ChannelId];
                }
                else
                {
                    rb = new CircularBuffer<Message>(Client.RestClient.Config.MessageCacheLength);
                    messages.Add(message.ChannelId, rb);
                }

                message.WithClient(Client);
                message.Guild = this;

                if (rb.FirstOrDefault(m => m.Id == message.Id) is Message oldMessage)
                    rb[rb.IndexOf(oldMessage)] = message;
                else
                    rb.Add(message);
            }
        }

        public void Remove(GuildEmoji emoji)
        {
            lock (emojiLock)
                emojis.Remove(emoji.Id);
        }

        public void Remove(Role role)
        {
            lock (roleLock)
                roles.Remove(role.Id);
        }

        public void Remove(GuildChannel channel)
        {
            lock (channelLock)
                channels.Remove(channel.Id);
        }

        public void Remove(GuildMember member)
        {
            lock (memberLock)
                members.Remove(member.Id);
        }

        public void Remove(Presence presence)
        {
            lock (presenceLock)
                presences.Remove(presence.User.Id);
        }

        public void Remove(Message message)
        {
            lock (messageLock)
                if (messages.TryGetValue(message.ChannelId, out var msgBuff))
                    msgBuff.Remove(message);
        }

        public void Replace(ReadOnlyCollection<GuildEmoji> newCollection)
        {
            lock (emojiLock)
            {
                emojis = new Dictionary<ulong, GuildEmoji>();

                foreach (var emoji in newCollection)
                {
                    emoji.WithClient(Client);
                    AddOrUpdate(emoji, x => x = emoji);
                }
            }
        }

        public ReadOnlyCollection<GuildEmoji> GetEmojis()
        {
            lock (emojiLock)
                return emojis.Select(x => x.Value).ToList().AsReadOnly();
        }
        
        public ReadOnlyCollection<Role> GetRoles()
        {
            lock (roleLock)
                return roles.Select(x => x.Value).ToList().AsReadOnly();
        }

        public ReadOnlyCollection<GuildChannel> GetChannels()
        {
            lock (channelLock)
                return channels.Select(x => x.Value).ToList().AsReadOnly();
        }

        public ReadOnlyCollection<Message> GetMessages(ulong channelId)
        {
            lock (messageLock)
                return messages.TryGetValue(channelId, out var messageCache)
                    ? messageCache.ToList().AsReadOnly()
                    : new ReadOnlyCollection<Message>(new List<Message>());
        }

        public ReadOnlyCollection<GuildMember> GetMembers()
        {
            lock (memberLock)
                return members.Select(x =>
                {
                    x.Value.Guild = Guild;
                    return x.Value;
                }).ToList().AsReadOnly();
        }

        public ReadOnlyCollection<Presence> GetPresences()
        {
            lock (presenceLock)
                return presences.Select(x => x.Value).ToList().AsReadOnly();
        }

        #endregion
    }
}