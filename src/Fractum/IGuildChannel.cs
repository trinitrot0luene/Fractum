﻿using System.Collections.ObjectModel;

namespace Fractum
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