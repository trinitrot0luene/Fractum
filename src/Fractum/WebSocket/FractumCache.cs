using System;
using System.Collections.Generic;
using System.Linq;
using Fractum;
using Fractum.WebSocket;

namespace Fractum.WebSocket
{
    public sealed class FractumCache
    {
        private readonly object guildLock = new object();
        private readonly object userLock = new object();
        private readonly object presenceLock = new object();
        private readonly object dmChannelLock = new object();

        public FractumSocketClient Client { get; }

        private Dictionary<ulong, SyncedGuildCache> guilds = new Dictionary<ulong, SyncedGuildCache>();
        private Dictionary<ulong, CachedDMChannel> dmChannels = new Dictionary<ulong, CachedDMChannel>();

        private Dictionary<ulong, CachedPresence> presences = new Dictionary<ulong, CachedPresence>();
        private Dictionary<ulong, User> users = new Dictionary<ulong, User>();

        internal FractumCache(FractumSocketClient client)
        {
            Client = client;
        }

        public IEnumerable<SyncedGuildCache> Guilds
        {
            get
            {
                lock (guildLock)
                    foreach (var guild in guilds)
                        yield return guild.Value;
            }
        }

        public IEnumerable<CachedDMChannel> DmChannels
        {
            get
            {
                lock (dmChannelLock)
                    foreach (var dmChannel in dmChannels)
                        yield return dmChannel.Value;
            }
        }

        public IEnumerable<User> Users
        {
            get
            {
                lock (userLock)
                    foreach (var user in users)
                        yield return user.Value;
            }
        }

        public bool TryGetGuild(ulong id, out SyncedGuildCache guild, SearchType searchType = SearchType.Guild)
        {
            lock (guildLock)
            {
                switch (searchType)
                {
                    case SearchType.Guild:
                        return guilds.TryGetValue(id, out guild);
                    case SearchType.Channel:
                        foreach (var gkvp in guilds)
                        {
                            if (gkvp.Value.TryGet(id, out CachedGuildChannel _))
                            {
                                guild = gkvp.Value;
                                return true;
                            }
                        }
                        break;
                }
            }
            guild = null;
            return false;
        }

        public bool TryGetDmChannel(ulong userId, out CachedDMChannel channel)
        {
            lock (dmChannelLock)
                return dmChannels.TryGetValue(userId, out channel);
        }

        public bool TryGetUser(ulong userId, out User user)
        {
            lock (userLock)
                return users.TryGetValue(userId, out user);
        }

        public bool TryGetPresence(ulong userId, out CachedPresence presence)
        {
            lock (presenceLock)
                return presences.TryGetValue(userId, out presence);
        }

        public bool HasGuild(ulong guildId)
        {
            lock (guildLock)
                return guilds.ContainsKey(guildId);
        }

        public bool HasDmChannel(ulong channelId)
        {
            lock (dmChannelLock)
                return dmChannels.ContainsKey(channelId);
        }

        public bool HasUser(ulong userId)
        {
            lock (userLock)
                return users.ContainsKey(userId);
        }

        public bool HasPresence(ulong userId)
        {
            lock (presenceLock)
                return presences.ContainsKey(userId);
        }

        public void AddOrReplace(SyncedGuildCache guild)
        {
            lock (guildLock)
                guilds[guild.Id] = guild;
        }

        public void AddOrReplace(CachedDMChannel channel)
        {
            lock (dmChannelLock)
                dmChannels[channel.Id] = channel;
        }

        public void AddOrReplace(User user)
        {
            lock (userLock)
                users[user.Id] = user;
        }

        public void AddOrReplace(CachedPresence presence)
        {
            lock (presenceLock)
                presences[presence.UserId] = presence;
        }

        public bool RemoveGuild(ulong guildId)
        {
            lock (guildLock)
                return guilds.Remove(guildId);
        }

        public bool RemoveDmChannel(ulong channelId)
        {
            lock (dmChannelLock)
                return dmChannels.Remove(channelId);
        }

        public bool RemoveUser(ulong userId)
        {
            lock (userLock)
                return users.Remove(userId);
        }

        public bool RemovePresence(ulong userId)
        {
            lock (presenceLock)
                return presences.Remove(userId);
        }

        public void Reset()
        {
            lock (guildLock)
                guilds = new Dictionary<ulong, SyncedGuildCache>();
            lock (dmChannelLock)
                dmChannels = new Dictionary<ulong, CachedDMChannel>();
            lock (userLock)
                users = new Dictionary<ulong, User>();
            lock (presenceLock)
                presences = new Dictionary<ulong, CachedPresence>();
        } 
    }
}