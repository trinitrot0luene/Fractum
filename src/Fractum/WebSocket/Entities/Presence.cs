using Fractum.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fractum.WebSocket.Entities
{
    public sealed class Presence
    {
        internal Presence()
        {
            Since = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            Activity = null;
            Status = Status.Online;
            Afk = false;
        }

        [JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
        internal User User { get; set; }

        [JsonProperty("since")]
        public long Since { get; set; }

        [JsonProperty("game")]
        public Activity Activity { get; set; }

        [JsonProperty("status")]
        private string StatusString { get; set; }

        [JsonIgnore]
        public Status Status
        {
            get
            {
                switch(StatusString)
                {
                    case "online":
                        return Status.Online;
                    case "dnd":
                        return Status.Dnd;
                    case "idle":
                        return Status.Idle;
                    case "invisible":
                        return Status.Invisible;
                    default:
                        return Status.Offline;
                }
            }
            set
            {
                switch(value)
                {
                    case Status.Online:
                        StatusString = "online";
                        return;
                    case Status.Dnd:
                        StatusString = "dnd";
                        return;
                    case Status.Idle:
                        StatusString = "idle";
                        return;
                    case Status.Invisible:
                        StatusString = "invisible";
                        return;
                    case Status.Offline:
                        StatusString = "offline";
                        return;
                }
            }
        }

        [JsonProperty("afk")]
        public bool Afk { get; set; }
    }
}
