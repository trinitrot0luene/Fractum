using Fractum.Entities;
using Fractum.WebSocket.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fractum.WebSocket.Pipelines
{
    public sealed class GuildCache
    {
        public ConcurrentDictionary<ulong, Presence> Presences { get; private set; }

        public ConcurrentDictionary<ulong, Emoji> Emojis { get; private set; }

        public ConcurrentDictionary<ulong, GuildMember> Members { get; private set; }

        public ConcurrentDictionary<ulong, GuildChannel> Channels { get; private set; }

        public ConcurrentDictionary<ulong, Role> Roles { get; private set; }

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

                foreach (var member in _cachedGuild.Members)
                    member.Presence = Presences.Select(kvp => kvp.Value).FirstOrDefault(p => p.User.Id == member.Id);

                return _cachedGuild;
            }
        }

        private Guild _cachedGuild;

        public FractumSocketClient Client { get; private set; }

        internal GuildCache(FractumSocketClient client, GuildCreateModel model)
        {
            Presences = new ConcurrentDictionary<ulong, Presence>();
            Emojis = new ConcurrentDictionary<ulong, Emoji>();
            Members = new ConcurrentDictionary<ulong, GuildMember>();
            Channels = new ConcurrentDictionary<ulong, GuildChannel>();
            Roles = new ConcurrentDictionary<ulong, Role>();

            Client = client;

            Create(model);
        }

        private void Create(GuildCreateModel model)
        {
            _cachedGuild = new Guild(model);

            foreach (var presence in model.Presences)
                Presences.TryAdd(presence.User.Id, presence);
            foreach (var emoji in model.Emojis)
                Emojis.TryAdd(emoji.Id, emoji.WithClient<Emoji>(Client.RestClient));
            foreach (var member in model.Members)
            {
                Members.TryAdd(member.Id, member.WithClient<GuildMember>(Client.RestClient));
                member.Guild = _cachedGuild;
            }
            foreach (var channel in model.Channels)
            {
                Channels.TryAdd(channel.Id, channel.WithClient<GuildChannel>(Client.RestClient));
                channel.Guild = _cachedGuild;
            }
            foreach (var role in model.Roles)
                Roles.TryAdd(role.Id, role.WithClient<Role>(Client.RestClient));
        }
    }
}
