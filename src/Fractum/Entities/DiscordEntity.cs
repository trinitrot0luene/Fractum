using System;
using Fractum.Entities.Contracts;
using Newtonsoft.Json;

namespace Fractum.Entities
{
    public abstract class DiscordEntity : FractumEntity, IEquatable<DiscordEntity>, IDiscordEntity
    {
        internal DiscordEntity()
        {
        }

        [JsonProperty("id")]
        public ulong Id { get; protected set; }

        [JsonIgnore]
        public DateTimeOffset CreatedAt =>
            new DateTimeOffset(2015, 1, 1, 0, 0, 0, TimeSpan.Zero).AddMilliseconds(Id >> 22);

        public virtual bool Equals(DiscordEntity other) => other.Id == Id;

        public override int GetHashCode() => Id.GetHashCode();
    }
}