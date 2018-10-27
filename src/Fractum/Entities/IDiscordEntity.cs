using System;

namespace Fractum.Entities
{
    public interface IDiscordEntity
    {
        ulong Id { get; }

        DateTimeOffset CreatedAt { get; }
    }
}