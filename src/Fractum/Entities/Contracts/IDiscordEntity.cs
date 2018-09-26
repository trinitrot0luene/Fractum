using System;
using System.Collections.Generic;
using System.Text;

namespace Fractum.Entities.Contracts
{
    public interface IDiscordEntity
    {
        ulong Id { get; }

        DateTimeOffset CreatedAt { get; }
    }
}
