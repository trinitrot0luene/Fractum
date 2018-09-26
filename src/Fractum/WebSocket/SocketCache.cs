using Fractum.Entities;
using Fractum.Entities.Contracts;
using Fractum.WebSocket.Events;
using Fractum.Rest;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fractum.WebSocket
{
    internal sealed class SocketCache
    {
        public ConcurrentDictionary<ulong, Guild> Guilds;

        public FractumRestClient RestClient;

        public SocketCache(FractumRestClient restClient)
        {
            Guilds = new ConcurrentDictionary<ulong, Guild>();
            RestClient = restClient;
        }

        public void Add(Guild guild)
            => Guilds.AddOrUpdate(guild.Id, guild.WithClient<Guild>(RestClient), (k, v) => 
            {
                guild.WithClient(RestClient);
                return guild;
            });

        public bool TryUpdateMembers(ulong guildId, IEnumerable<GuildMember> members)
        {
            if (Guilds.TryGetValue(guildId, out var guild))
            {
                guild.Members = members.ToList().AsReadOnly();
                return true;
            }

            return false;
        }

        public void PopulateMessage(Message msg)
        {
            if (msg.GuildId.HasValue && Guilds.TryGetValue(msg.GuildId.Value, out var guild))
            {   
                msg.Guild = guild.WithClient<Guild>(RestClient);
                msg.Channel = guild.TextChannels.FirstOrDefault(c => c.Id == msg.ChannelId); // TODO: DM Channels

                msg.Author = guild.Members.FirstOrDefault(m => m.Id == msg.MessageAuthor.Id) as IUser ?? msg.MessageAuthor;
            }
        }

        public void UpdateStatus(PresenceUpdateEvent presenceUpdate)
        {
            if (Guilds.TryGetValue(presenceUpdate.GuildId.Value, out var guild))
                presenceUpdate.ApplyToGuild(guild);
        }
    }
}
