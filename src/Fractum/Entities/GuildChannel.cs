using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Fractum.Entities.Properties;
using Newtonsoft.Json;

namespace Fractum.Entities
{
    public class GuildChannel : Channel
    {
        internal GuildChannel()
        {
        }

        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("position")]
        public int Position { get; private set; }

        [JsonProperty("guild_id")]
        internal ulong GuildId { get; private set; }

        [JsonProperty("permission_overwrites")]
        public ReadOnlyCollection<PermissionsOverwrite> Overwrites { get; private set; }

        [JsonIgnore]
        public Guild Guild { get; internal set; }

        [JsonProperty("parent_id")]
        public ulong? ParentId { get; internal set; }

        [JsonProperty("nsfw")]
        public bool IsNsfw { get; private set; }

        public override string ToString() => $"{Name} : {Id}";

        public Task DeleteAsync()
            => Client.RestClient.DeleteChannelAsync(Id);

        public virtual async Task<GuildChannel> EditAsync(Action<GuildChannelProperties> editAction)
        {
            var props = new GuildChannelProperties()
            {
                Name = this.Name,
                PermissionsOverwrites = this.Overwrites,
                Position = this.Position
            };
            editAction(props);

            return await Client.EditChannelAsync(Id, props);
        }

        private Permissions ComputeBasePermissions(GuildMember member)
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

        private Permissions ComputeOverwritePermissions(Permissions base_permissions, GuildMember member)
        {
            if (base_permissions.HasFlag(Permissions.Administrator))
                return Permissions.All;

            var permissions = base_permissions;
            var everyone_overwrite =
                Overwrites.FirstOrDefault(o => o.Id == Guild.Roles.First(r => r.Name == "@everyone").Id);
            if (everyone_overwrite != null)
            {
                permissions &= ~everyone_overwrite.Deny;
                permissions |= ~everyone_overwrite.Allow;
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

        internal Permissions ComputePermissions(GuildMember member)
            => ComputeOverwritePermissions(ComputeBasePermissions(member), member);
    }
}