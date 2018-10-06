using System;
using System.Collections.Concurrent;
using Fractum.Entities;

namespace Fractum.WebSocket.Core
{
    public sealed class FractumCache
    {
        public FractumCache(FractumSocketClient client)
        {
            Guilds = new ConcurrentDictionary<ulong, GuildCache>();
            Client = client;
        }

        public ConcurrentDictionary<ulong, GuildCache> Guilds { get; internal set; }

        public FractumSocketClient Client { get; }

        public Guild GetGuild(ulong guildId)
            => Guilds.TryGetValue(guildId, out var guild) ? guild.Value : default;

        public void UpdateGuildCache(ulong guildId, Action<GuildCache> update)
        {
            if (Guilds.TryGetValue(guildId, out var guild))
                update(guild);
        }

        public void AddAndPopulateMessage(Message message)
        {
            var guild = GetGuild(message.GuildId ?? 0);
            message.Guild = guild;
            message.WithClient(Client);
        }

        public void Clear()
            => Guilds = new ConcurrentDictionary<ulong, GuildCache>();
    }
}