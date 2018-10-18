using System;

namespace Fractum.Contracts
{
    public interface IDiscordEntity
    {
        ulong Id { get; }

        DateTimeOffset CreatedAt { get; }
    }
}