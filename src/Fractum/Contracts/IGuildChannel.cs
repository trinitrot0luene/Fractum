using System.Collections.ObjectModel;
using Fractum.Entities;

namespace Fractum.Contracts
{
    public interface IGuildChannel : IDiscordEntity
    {
        string Name { get; }

        ChannelType Type { get; }

        int Position { get; }

        ulong GuildId { get; }

        ulong? ParentId { get; }

        ReadOnlyCollection<PermissionsOverwrite> Overwrites { get; }
    }
}