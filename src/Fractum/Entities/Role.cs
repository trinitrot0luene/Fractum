using System.Drawing;
using Newtonsoft.Json;

namespace Fractum.Entities
{
    public sealed class Role : DiscordEntity
    {
        [JsonProperty("position")]
        public int Position { get; internal set; }

        [JsonProperty("permissions")]
        public Permissions Permissions { get; internal set; }

        [JsonProperty("name")]
        public string Name { get; internal set; }

        [JsonProperty("mentionable")]
        public bool IsMentionable { get; internal set; }

        [JsonProperty("managed")]
        public bool IsManaged { get; internal set; }

        [JsonProperty("hoisted")]
        public bool IsHoisted { get; internal set; }

        [JsonIgnore]
        public string Mention
        {
            get => string.Format(Consts.ROLE_MENTION, Id);
        }

        [JsonIgnore]
        public Color Color => Color.FromArgb(ColorRaw);

        [JsonProperty("color")]
        internal int ColorRaw { get; set; }
    }
}