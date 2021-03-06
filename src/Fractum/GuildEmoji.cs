﻿using Fractum.WebSocket;
using Fractum.WebSocket.EventModels;

namespace Fractum
{
    public sealed class GuildEmoji : PopulatedEntity
    {
        internal GuildEmoji(GatewayCache cache, GuildEmojiModel model) : base(cache)
        {
            Id = model.Id;
            CreatorId = model.Creator?.Id;
            Name = model.Name;
            RoleIds = model.RoleIds;
            RequiresColons = model.RequiresColons;
            IsManaged = model.IsManaged;
            IsAnimated = model.IsAnimated;
        }

        private ulong? CreatorId { get; }

        public User Creator => Cache.TryGetUser(CreatorId ?? 0, out var user) ? user : default;

        public string Name { get; }

        public ulong[] RoleIds { get; }

        public bool RequiresColons { get; }

        public bool IsManaged { get; }

        public bool IsAnimated { get; }
    }
}