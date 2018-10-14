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
            Presences = new List<Presence>();
            Emojis = new List<GuildEmoji>();
            Members = new List<GuildMember>();
            Channels = new List<GuildChannel>();
            Roles = new List<Role>();
            Messages = new Dictionary<ulong, CircularBuffer<Message>>();

            Client = client;

            Create(model);
        }

        public List<Presence> Presences { get; }

        public List<GuildEmoji> Emojis { get; }

        public List<GuildMember> Members { get; }

        public List<GuildChannel> Channels { get; }

        public List<Role> Roles { get; }

        public Dictionary<ulong, CircularBuffer<Message>> Messages { get; }

        public Guild Value
        {
            get
            {
                _cachedGuild.Channels = Channels.AsReadOnly();
                _cachedGuild.Roles = Roles.AsReadOnly();
                _cachedGuild.Emoji = Emojis.AsReadOnly();
                _cachedGuild.Presences = Presences.AsReadOnly();
                _cachedGuild.Members = Members.AsReadOnly();

                foreach (var chn in _cachedGuild.Channels)
                    if (chn is TextChannel tc)
                        tc.Messages = Messages.TryGetValue(tc.Id, out var messages)
                            ? messages.ToList().AsReadOnly()
                            : new List<Message>().AsReadOnly();

                foreach (var member in _cachedGuild.Members)
                    member.Presence = Presences
                        .FirstOrDefault(p => p.User.Id == member.Id);

                return _cachedGuild;
            }
        }

        public FractumSocketClient Client { get; }

        internal void UpdateGuild(Action<Guild> updateAction)
            => updateAction(_cachedGuild);

        private void Create(GuildCreateEventModel model)
        {
            _cachedGuild = new Guild(model)
                .WithClient<Guild>(Client);

            foreach (var presence in model.Presences)
                Presences.Add(presence);
            foreach (var emoji in model.Emojis)
                Emojis.Add(emoji.WithClient<GuildEmoji>(Client));
            foreach (var member in model.Members)
            {
                Members.Add(member.WithClient<GuildMember>(Client));
                member.Guild = _cachedGuild;
            }

            foreach (var channel in model.Channels)
            {
                Channels.Add(channel.WithClient<GuildChannel>(Client));
                channel.Guild = _cachedGuild;

                var newbuff = new CircularBuffer<Message>(Client.RestClient.Config.MessageCacheLength);
                Messages[channel.Id] = newbuff;
            }

            foreach (var role in model.Roles)
                Roles.Add(role.WithClient<Role>(Client));
        }
    }
}