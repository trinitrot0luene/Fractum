using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels.Entities;

namespace Fractum.Entities
{
    public sealed class GuildEmoji : PopulatedEntity
    {
        internal GuildEmoji(FractumCache cache, GuildEmojiModel model) : base(cache)
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

        public User Creator => Cache.GetUserOrDefault(CreatorId ?? 0);

        public string Name { get; }

        public ulong[] RoleIds { get; }

        public bool RequiresColons { get; }

        public bool IsManaged { get; }

        public bool IsAnimated { get; }
    }
}