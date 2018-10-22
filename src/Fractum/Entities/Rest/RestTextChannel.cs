using System;
using Newtonsoft.Json;

namespace Fractum.Entities.Rest
{
    public class RestTextChannel : RestGuildChannel
    {
        internal RestTextChannel()
        {
        }

        [JsonProperty("nsfw")]
        public bool IsNsfw { get; private set; }

        [JsonProperty("topic")]
        public string Topic { get; private set; }

        [JsonProperty("last_pin_timestamp")]
        public DateTimeOffset? LastPinAt { get; private set; }

        [JsonProperty("rate_limit_per_user")]
        public int PerUserRatelimit { get; private set; }

        [JsonProperty("last_message_id")]
        public ulong? LastMessageId { get; internal set; }
    }
}