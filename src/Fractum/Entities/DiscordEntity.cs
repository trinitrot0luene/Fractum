using Newtonsoft.Json;
using Fractum.Entities.Contracts;
using Fractum.Rest;
using System;
using System.Collections.Generic;
using System.Text;
using Fractum.WebSocket;

namespace Fractum.Entities
{
    public abstract class DiscordEntity : FractumEntity, IEquatable<DiscordEntity>, IDiscordEntity
    {
        internal DiscordEntity() { }

        [JsonProperty("id")]
        public ulong Id { get; protected set; }

        [JsonIgnore]
        public DateTimeOffset CreatedAt { get => new DateTimeOffset(2015, 1, 1, 0, 0, 0, TimeSpan.Zero).AddMilliseconds(Id >> 22); }

        public virtual bool Equals(DiscordEntity other) => other.Id == Id;

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
