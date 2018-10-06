using System;

namespace Fractum.Entities.Contracts
{
    public interface IDiscordEntity
    {
        ulong Id { get; }

        DateTimeOffset CreatedAt { get; }
    }
}