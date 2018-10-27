using System.Collections.Generic;
using Fractum.Entities;
using Fractum.Entities.WebSocket;

namespace Fractum.WebSocket
{
    public interface ISocketCache<T> where T : ISyncedGuild
    {
        FractumSocketClient Client { get; }

        IEnumerable<ISyncedGuild> Guilds { get; }

        bool TryGetGuild(ulong id, out T guild, SearchType searchType = SearchType.Guild);

        bool TryGetUser(ulong userId, out User user);

        bool TryGetPresence(ulong userId, out CachedPresence presence);

        bool HasGuild(ulong guildId);

        bool HasUser(ulong userId);

        bool HasPresence(ulong userId);

        void AddOrReplace(User user);

        void AddOrReplace(CachedPresence presence);

        void AddOrReplace(T guild);

        bool RemoveGuild(ulong guildId);

        bool RemoveUser(ulong userId);

        bool RemovePresence(ulong userId);

        void Reset();
    }
}