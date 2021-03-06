﻿using System;
using Newtonsoft.Json;

namespace Fractum
{
    public abstract class DiscordEntity : IEquatable<DiscordEntity>, IDiscordEntity
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