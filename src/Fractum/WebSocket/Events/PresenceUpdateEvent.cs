using Fractum.Entities;
using Fractum.WebSocket.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fractum.WebSocket.Events
{
    internal sealed class PresenceUpdateEvent
    {
        [JsonProperty("user")]
        public User User { get; private set; }

        [JsonProperty("roles")]
        public ulong[] Roles { get; private set; }

        [JsonProperty("nick")]
        public string Nickname { get; private set; }

        [JsonProperty("game")]
        public Activity Activity { get; private set; }

        [JsonProperty("activities")]
        public Activity[] Activities { get; private set; }

        [JsonProperty("guild_id")]
        public ulong? GuildId { get; private set; }

        [JsonProperty("status")]
        private string StatusRaw { get; set; }

        [JsonIgnore]
        public Status? NewStatus {
            get
            {
                switch (StatusRaw)
                {
                    case "online":
                        return Status.Online;
                    case "idle":
                        return Status.Idle;
                    case "dnd":
                        return Status.Dnd;
                    case "offline":
                        return Status.Offline;
                    default:
                        return null;
                }
            }
        }
        
        public void ApplyToGuild(Guild guild)
        {
            var member = guild.Members.FirstOrDefault(m => m.Id == User.Id);
            if (member is null)
                return;
            member.RoleIds = Roles ?? member.RoleIds;
            member.IsDeafened = User.Member?.IsDeafened ?? member.IsDeafened;
            member.IsMuted = User.Member?.IsMuted ?? member.IsMuted;
            member.Nickname = Nickname ?? member.Nickname;
            member.User.Username = User.Username ?? member.User.Username;
            member.User.Discrim = User.Discrim != short.MinValue ? User.Discrim : member.User.Discrim;

            var newPresence = new Presence();
            newPresence.Activity = Activity;
            if (NewStatus.HasValue)
                newPresence.Status = NewStatus.Value;

            guild.PresenceData.AddOrUpdate(member.Id, newPresence, (id, presence) =>
            {
                presence.Activity = Activity;
                presence.Status = NewStatus ?? presence.Status;
                return presence;
            });
        }
    }
}
