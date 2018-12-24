using System;

namespace Fractum
{
    public interface IDiscordEntity
    {
        ulong Id { get; }

        DateTimeOffset CreatedAt { get; }
    }
}