using System.Collections.ObjectModel;
using Fractum.Contracts;
using Newtonsoft.Json;

namespace Fractum.Entities.Rest
{
    public abstract class RestGuildChannel : RestChannel, IGuildChannel
    {
        internal RestGuildChannel()
        {
        }

        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("position")]
        public int Position { get; private set; }

        [JsonProperty("guild_id")]
        public ulong GuildId { get; private set; }

        [JsonProperty("parent_id")]
        public ulong? ParentId { get; private set; }

        [JsonProperty("permission_overwrites")]
        public ReadOnlyCollection<PermissionsOverwrite> Overwrites { get; private set; }
    }
}