using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Fractum.Entities;
using Fractum.Entities.WebSocket;

namespace Fractum.WebSocket.Core
{
    public sealed class FractumCache
    {
        private readonly object guildLock = new object();
        private readonly object userLock = new object();

        internal FractumSocketClient Client;

        private Dictionary<ulong, SyncedGuildCache> guilds = new Dictionary<ulong, SyncedGuildCache>();
        private readonly Dictionary<ulong, User> users = new Dictionary<ulong, User>();

        public FractumCache(FractumSocketClient client)
        {
            Client = client;
        }

        public ReadOnlyCollection<SyncedGuildCache> Guilds
        {
            get
            {
                lock (guildLock)
                {
                    return guilds.Select(x => x.Value).ToList().AsReadOnly();
                }
            }
        }

        public SyncedGuildCache this[ulong id]
        {
            get => guilds.TryGetValue(id, out var cache) ? cache : default;
            set
            {
                lock (guildLock)
                {
                    guilds[id] = value;
                }
            }
        }

        public SyncedGuildCache this[CachedMessage msg] => guilds.TryGetValue(msg.GuildId ?? 0, out var cache)
            ? cache
            : guilds.First(x => x.Value.GetChannels().Any(c => c.Id == msg.ChannelId)).Value;

        public bool HasGuild(ulong id)
            => guilds.ContainsKey(id);

        public void Remove(ulong id)
        {
            lock (guildLock)
            {
                guilds.Remove(id);
            }
        }

        public void AddUser(User user)
        {
            lock (userLock)
            {
                if (!users.ContainsKey(user.Id))
                    users.Add(user.Id, user);
            }
        }

        public void RemoveUser(ulong id)
        {
            lock (userLock)
            {
                users.Remove(id);
            }
        }

        public User GetUserOrDefault(ulong id)
        {
            lock (userLock)
            {
                return users.TryGetValue(id, out var user) ? user : default;
            }
        }

        public void AddOrUpdate(SyncedGuildCache newValue, Func<SyncedGuildCache, SyncedGuildCache> replaceAction)
        {
            lock (guildLock)
            {
                if (guilds.ContainsKey(newValue.Id))
                    guilds[newValue.Id] = replaceAction(guilds[newValue.Id]);
                else
                    guilds.Add(newValue.Id, newValue);
            }
        }

        public void Clear()
            => guilds = new Dictionary<ulong, SyncedGuildCache>();
    }
}