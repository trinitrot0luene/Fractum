using Fractum.Entities;
using Fractum.Entities.WebSocket;
using Fractum.WebSocket.Core;
using Newtonsoft.Json;
using System.Linq;
using Fractum.Utilities;

namespace Fractum.WebSocket.EventModels
{
    internal sealed class PresenceUpdateEventModel : EventModelBase
    {
        [JsonProperty("user")]
        public User User { get; private set; }

        [JsonProperty("member")]
        public PartialMember PartialMember { get; private set; }

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
        public Status? NewStatus
        {
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

        public override void ApplyToCache(FractumCache cache)
        {
            if (GuildId.HasValue && cache.HasGuild(GuildId.Value))
            {
                var guild = cache[GuildId.Value];
                if (guild.GetMembers().FirstOrDefault(m => m.Id == User.Id) is GuildMember member)
                {
                    member.RoleIds = Roles ?? member.RoleIds;
                    member.IsDeafened = User.Member?.IsDeafened ?? member.IsDeafened;
                    member.IsMuted = User.Member?.IsMuted ?? member.IsMuted;
                    member.Nickname = Nickname ?? member.Nickname;
                    member.User.Username = User.Username ?? member.User.Username;
                    member.User.Discrim = User.Discrim != short.MinValue ? User.Discrim : member.User.Discrim;
                }

                var newPresence = new Presence();
                newPresence.Activity = Activity;
                newPresence.User = User;
                if (NewStatus.HasValue)
                    newPresence.Status = NewStatus.Value;

                guild.AddOrUpdate(newPresence, old => {
                    old.Activity = Activity;
                    old.Status = NewStatus ?? old.Status;
                    return old;
                });
            }
        }
    }
}