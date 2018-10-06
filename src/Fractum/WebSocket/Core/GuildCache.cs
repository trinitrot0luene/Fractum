using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Fractum.Entities;
using Fractum.Entities.WebSocket;
using Fractum.Rest;
using Fractum.Utilities;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Core
{
    public sealed class GuildCache
    {
        private Guild _cachedGuild;

        internal GuildCache(FractumSocketClient client, GuildCreateEventModel model)
        {
            Presences = new ConcurrentDictionary<ulong, Presence>();
            Emojis = new ConcurrentDictionary<ulong, Emoji>();
            Members = new ConcurrentDictionary<ulong, GuildMember>();
            Channels = new ConcurrentDictionary<ulong, GuildChannel>();
            Roles = new ConcurrentDictionary<ulong, Role>();
            Messages = new ConcurrentDictionary<ulong, CircularBuffer<Message>>();

            Client = client;

            Create(model);
        }

        public ConcurrentDictionary<ulong, Presence> Presences { get; }

        public ConcurrentDictionary<ulong, Emoji> Emojis { get; }

        public ConcurrentDictionary<ulong, GuildMember> Members { get; }

        public ConcurrentDictionary<ulong, GuildChannel> Channels { get; }

        public ConcurrentDictionary<ulong, Role> Roles { get; }

        public ConcurrentDictionary<ulong, CircularBuffer<Message>> Messages { get; }

        public Guild Value
        {
            get
            {
                _cachedGuild.Channels = Channels.Select(kvp => kvp.Value)
                    .ToList().AsReadOnly();
                _cachedGuild.Roles = Roles.Select(kvp => kvp.Value)
                    .ToList().AsReadOnly();
                _cachedGuild.Emoji = Emojis.Select(kvp => kvp.Value)
                    .ToList().AsReadOnly();
                _cachedGuild.Presences = Presences.Select(kvp => kvp.Value)
                    .ToList().AsReadOnly();
                _cachedGuild.Members = Members.Select(kvp => kvp.Value)
                    .ToList().AsReadOnly();

                foreach (var chn in _cachedGuild.Channels)
                    if (chn is TextChannel tc)
                        tc.Messages = Messages.TryGetValue(tc.Id, out var messages)
                            ? messages.ToList().AsReadOnly()
                            : new List<Message>().AsReadOnly();

                foreach (var member in _cachedGuild.Members)
                    member.Presence = Presences.Select(kvp => kvp.Value)
                        .FirstOrDefault(p => p.User.Id == member.Id);

                return _cachedGuild;
            }
        }

        public FractumRestClient Client { get; }

        internal void UpdateGuild(Action<Guild> updateAction)
            => updateAction(_cachedGuild);

        private void Create(GuildCreateEventModel model)
        {
            _cachedGuild = new Guild(model)
                .WithClient<Guild>(Client);

            foreach (var presence in model.Presences)
                Presences.TryAdd(presence.User.Id, presence);
            foreach (var emoji in model.Emojis)
                Emojis.TryAdd(emoji.Id, emoji.WithClient<Emoji>(Client));
            foreach (var member in model.Members)
            {
                Members.TryAdd(member.Id, member.WithClient<GuildMember>(Client));
                member.Guild = _cachedGuild;
            }

            foreach (var channel in model.Channels)
            {
                Channels.TryAdd(channel.Id, channel.WithClient<GuildChannel>(Client));
                channel.Guild = _cachedGuild;

                var newbuff = new CircularBuffer<Message>(Client.Config.MessageCacheLength);
                Messages.AddOrUpdate(channel.Id, newbuff, (k, v) => v = newbuff);
            }

            foreach (var role in model.Roles)
                Roles.TryAdd(role.Id, role.WithClient<Role>(Client));
        }
    }
}