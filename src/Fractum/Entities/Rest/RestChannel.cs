using System;
using Newtonsoft.Json;

namespace Fractum.Entities.Rest
{
    public abstract class RestChannel
    {
        internal RestChannel()
        {
        }

        [JsonProperty("id")]
        public ulong Id { get; private set; }

        [JsonProperty("type")]
        public ChannelType Type { get; private set; }

        [JsonIgnore]
        public DateTimeOffset CreatedAt =>
            new DateTimeOffset(2015, 1, 1, 0, 0, 0, TimeSpan.Zero).AddMilliseconds(Id >> 22);
    }
}