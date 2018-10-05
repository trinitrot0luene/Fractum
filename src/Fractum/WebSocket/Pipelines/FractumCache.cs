using System;
using System.Collections.Concurrent;
using Fractum.Entities;
using Fractum.WebSocket.Entities;

namespace Fractum.WebSocket.Pipelines
{
    public sealed class FractumCache
    {
        public FractumCache(FractumSocketClient client)
        {
            Guilds = new ConcurrentDictionary<ulong, GuildCache>();
            Client = client;
        }

        public ConcurrentDictionary<ulong, GuildCache> Guilds { get; private set; }

        public FractumSocketClient Client { get; }

        public Guild GetGuild(ulong guildId)
            => Guilds.TryGetValue(guildId, out var guild) ? guild.Value : default;

        public void AddGuild(GuildCreateModel model)
        {
            var cache = new GuildCache(Client, model);
            Guilds.AddOrUpdate(model.Id, cache, (k, v) => v = cache);
        }

        public void UpdateGuildCache(ulong guildId, Action<GuildCache> update)
        {
            if (Guilds.TryGetValue(guildId, out var guild))
                update(guild);
        }

        public void AddAndPopulateMessage(Message message)
        {
            var guild = GetGuild(message.GuildId ?? 0);
            message.Guild = guild;
            message.WithClient(Client.RestClient);
        }

        public void Clear()
            => Guilds = new ConcurrentDictionary<ulong, GuildCache>();
    }
}