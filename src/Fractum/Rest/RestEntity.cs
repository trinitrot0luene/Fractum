using System;
using Fractum.Rest;
using Newtonsoft.Json;

namespace Fractum.Rest
{
    public abstract class RestEntity
    {
        internal RestEntity()
        {
        }

        internal FractumRestClient Client { get; set; }


        [JsonProperty("id")]
        public ulong Id { get; protected set; }

        [JsonIgnore]
        public DateTimeOffset CreatedAt =>
            new DateTimeOffset(2015, 1, 1, 0, 0, 0, TimeSpan.Zero).AddMilliseconds(Id >> 22);
    }
}