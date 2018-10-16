using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Fractum.Contracts;
using Fractum.Entities;

namespace Fractum.WebSocket.Core
{
    public sealed class FractumCache
    {
        private readonly object guildLock = new object();

        private Dictionary<ulong, GuildCache> guilds = new Dictionary<ulong, GuildCache>();

        internal FractumSocketClient Client;

        public FractumCache(FractumSocketClient client)
        {
            Client = client;
        }

        public bool HasGuild(ulong id)
            => guilds.ContainsKey(id);

        public void Remove(ulong id)
        {
            lock (guildLock)
                guilds.Remove(id);
        }

        public ReadOnlyCollection<GuildCache> Guilds
        {
            get
            {
                lock (guildLock)
                    return guilds.Select(x => x.Value).ToList().AsReadOnly();
            }
        }

        public void Populate(Message message)
        {
            var guild = Guilds.FirstOrDefault(g => g.GetChannels().Any(c => c.Id == message.ChannelId));
            if (guild is null)
                return;

            message.Channel = guild.GetChannels().First(x => x.Id == message.ChannelId) as IMessageChannel;
            message.MentionedRoles = guild.GetRoles()
                .Where(role => message.MentionedRoleIds.Any(rid => rid == role.Id)).ToList().AsReadOnly();
            message.WithClient(Client);
        }

        public void Populate(GuildChannel channel)
        {
            var guild = this[channel.GuildId];
            if (guild is null)
                return;

            channel.Guild = guild.Guild;
            channel.WithClient(Client);
        }

        public void AddOrUpdate(GuildCache newValue, Func<GuildCache, GuildCache> replaceAction)
        {
            lock (guildLock)
            {
                if (guilds.ContainsKey(newValue.Id))
                    guilds[newValue.Id] = replaceAction(guilds[newValue.Id]);
                else
                    guilds.Add(newValue.Id, newValue);
            }
        }

        public GuildCache this[ulong id]
        {
            get => guilds.TryGetValue(id, out var cache) ? cache : default;
            set
            {
                lock (guildLock)
                    guilds[id] = value;
            }
        }

        public void Clear()
            => guilds = new Dictionary<ulong, GuildCache>();
    }
}