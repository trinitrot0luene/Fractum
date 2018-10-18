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

        public void Populate(GuildChannel channel)
        {
            var guild = this[channel.GuildId];
            if (guild is null)
                return;

            channel.Cache = guild;
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

        public GuildCache this[Message msg]
        {
            get => guilds.TryGetValue(msg.GuildId ?? 0, out var cache)
                    ? cache
                    : guilds.First(x => x.Value.GetChannels().Any(c => c.Id == msg.ChannelId)).Value;
        }

        public void Clear()
            => guilds = new Dictionary<ulong, GuildCache>();
    }
}