using Fractum.Entities;
using Fractum.Entities.WebSocket;
using Newtonsoft.Json;

namespace Fractum.WebSocket.EventModels
{
    public class PresenceUpdateEventModel : EventModelBase
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
    }
}