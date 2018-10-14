using Fractum.Entities;
using Fractum.Entities.WebSocket;
using Fractum.WebSocket.Core;
using Newtonsoft.Json;
using System.Linq;
using Fractum.Utilities;

namespace Fractum.WebSocket.EventModels
{
    internal sealed class PresenceUpdateEventModel : BaseEventModel
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
            cache.UpdateGuildCache(GuildId ?? 0, guildCache =>
            {
                if (guildCache.Members.FirstOrDefault(m => m.Id == User.Id) is GuildMember member)
                {
                    member.RoleIds = Roles ?? member.RoleIds;
                    member.IsDeafened = User.Member?.IsDeafened ?? member.IsDeafened;
                    member.IsMuted = User.Member?.IsMuted ?? member.IsMuted;
                    member.Nickname = Nickname ?? member.Nickname;
                    member.User.Username = User.Username ?? member.User.Username;
                    member.User.Discrim = User.Discrim != short.MinValue ? User.Discrim : member.User.Discrim;

                    var newPresence = new Presence();
                    newPresence.Activity = Activity;
                    newPresence.User = User;
                    if (NewStatus.HasValue)
                        newPresence.Status = NewStatus.Value;

                    guildCache.Presences.AddOrUpdate((a, b) => a.User.Id == b.User.Id, newPresence, oldPresence => 
                    {
                        oldPresence.Activity = Activity;
                        oldPresence.Status = NewStatus ?? oldPresence.Status;
                    });
                }
            });
        }
    }
}