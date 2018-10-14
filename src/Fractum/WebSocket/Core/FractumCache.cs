using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Fractum.Contracts;
using Fractum.Entities;

namespace Fractum.WebSocket.Core
{
    public sealed class FractumCache : IEnumerable<GuildCache>
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

        public void PopulateMessage(Message message)
        {
            var channel = Guilds.SelectMany(kvp => kvp.Value.Channels)
                .FirstOrDefault(c => c.Id == message.ChannelId);

            var msgChannel = channel as IMessageChannel ?? (Client.PrivateChannels.TryGetValue(message.ChannelId, out var dmChannel) ? dmChannel : default);

            message.Channel = msgChannel;

            message.WithClient(Client);
        }

        public void PopulateChannel(GuildChannel channel)
        {
            var existingChannel = Guilds[channel.GuildId].Channels.FirstOrDefault(c => c.Id == channel.Id);
            channel.Guild = existingChannel.Guild;
            if (channel is TextChannel txtChannel)
                (channel as TextChannel).Messages = (existingChannel as TextChannel)?.Messages;

            existingChannel.WithClient(Client);
        }

        public void Clear()
            => Guilds = new ConcurrentDictionary<ulong, GuildCache>();

        IEnumerator<GuildCache> IEnumerable<GuildCache>.GetEnumerator() 
                => Guilds.Select(cachedGuild => cachedGuild.Value).GetEnumerator();

        public IEnumerator GetEnumerator()
            => Guilds.Select(cachedGuild => cachedGuild.Value).GetEnumerator();
    }
}