using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Fractum.Rest;
using Fractum.WebSocket;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket
{
    public abstract class CachedGuildChannel : CachedChannel, IGuildChannel
    {
        internal CachedGuildChannel(GatewayCache cache, ChannelCreateUpdateOrDeleteEventModel model,
            ulong? guildId = null) : base(cache)
        {
            Id = model.Id;

            GuildId = guildId ?? model.GuildId;
            ParentId = model.ParentId;

            Name = model.Name;
            Position = model.Position;
            Overwrites = model.Overwrites;
            IsNsfw = model.IsNsfw;

            Type = model.Type;

            if (cache.TryGetGuild(Id, out var guild, SearchType.Channel))
                guild.AddOrReplace(this);
        }

        internal void Update(ChannelCreateUpdateOrDeleteEventModel model)
        {
            GuildId = model.GuildId;
            ParentId = model.ParentId;

            Name = model.Name;
            Position = model.Position;
            Overwrites = model.Overwrites;
            IsNsfw = model.IsNsfw;
        }

        public override string ToString() => $"{Name} : {Id}";

        #region Populated Properties

        public CachedGuild Guild => Cache.TryGetGuild(GuildId, out var guild) ? guild.Guild : default;

        public ulong GuildId { get; private set; }

        public CachedCategory Category => Cache.TryGetGuild(GuildId, out var guild)
            ? guild.TryGet(ParentId ?? 0, out CachedGuildChannel channel) ? channel as CachedCategory : default : default;

        public ulong? ParentId { get; private set; }

        #endregion

        #region Cached Properties 

        public string Name { get; private set; }

        public int Position { get; private set; }

        public ReadOnlyCollection<PermissionsOverwrite> Overwrites { get; private set; }

        public bool IsNsfw { get; private set; }

        #endregion

        #region REST

        public Task DeleteAsync()
            => Client.RestClient.DeleteChannelAsync(Id);

        public virtual async Task<RestGuildChannel> EditAsync(Action<GuildChannelProperties> editAction)
        {
            var props = new GuildChannelProperties
            {
                Name = Name,
                PermissionsOverwrites = Overwrites,
                Position = Position
            };
            editAction(props);

            return await Client.RestClient.EditChannelAsync(Id, props);
        }

        #endregion

        #region Permissions

        private Permissions ComputeBasePermissions(CachedMember member)
        {
            if (Guild.OwnerId == member.Id)
                return Permissions.All;
            if (member.Roles.Any(r => r.Permissions.HasFlag(Permissions.Administrator)))
                return Permissions.All;

            var everyone_role = Guild.Roles.First(r => r.Name == "@everyone");
            var member_permissions = everyone_role.Permissions;

            foreach (var role in member.Roles)
                member_permissions |= role.Permissions;

            if (member_permissions.HasFlag(Permissions.Administrator))
                return Permissions.All;
            return member_permissions;
        }

        private Permissions ComputeOverwritePermissions(Permissions base_permissions, CachedMember member)
        {
            if (base_permissions.HasFlag(Permissions.Administrator))
                return Permissions.All;

            var permissions = base_permissions;
            var everyone_overwrite =
                Overwrites.FirstOrDefault(o => o.Id == Guild.Roles.First(r => r.Name == "@everyone").Id);
            if (everyone_overwrite != null)
            {
                permissions &= ~everyone_overwrite.Deny;
                permissions |= everyone_overwrite.Allow;
            }

            var allow = Permissions.None;
            var deny = Permissions.None;

            var role_overwrites = Overwrites.Where(o => member.Roles.Any(r => r.Id == o.Id));
            foreach (var role_overwrite in role_overwrites)
            {
                allow |= role_overwrite.Allow;
                deny |= role_overwrite.Deny;
            }

            permissions &= ~deny;
            permissions |= allow;

            var member_overwrite = Overwrites.FirstOrDefault(o => o.Id == member.Id);
            if (member_overwrite != null)
            {
                permissions &= ~member_overwrite.Deny;
                permissions |= member_overwrite.Allow;
            }

            return permissions;
        }

        public Permissions ComputePermissions(CachedMember member)
            => ComputeOverwritePermissions(ComputeBasePermissions(member), member);

        #endregion
    }
}