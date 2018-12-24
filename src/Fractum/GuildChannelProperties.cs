using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace Fractum
{
    public class GuildChannelProperties
    {
        internal GuildChannelProperties()
        {
        }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
        public int? Position { get; set; }

        [JsonProperty("permission_overwrites", NullValueHandling = NullValueHandling.Ignore)]
        public ReadOnlyCollection<PermissionsOverwrite> PermissionsOverwrites { get; set; }
    }
}