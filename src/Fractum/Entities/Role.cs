using System.Drawing;
using Newtonsoft.Json;

namespace Fractum.Entities
{
    public sealed class Role : DiscordEntity
    {
        internal Role()
        {
        }

        [JsonProperty("position")]
        public int Position { get; private set; }

        [JsonProperty("permissions")]
        public Permissions Permissions { get; private set; }

        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("mentionable")]
        public bool IsMentionable { get; private set; }

        [JsonProperty("managed")]
        public bool IsManaged { get; private set; }

        [JsonProperty("hoisted")]
        public bool IsHoisted { get; private set; }

        [JsonProperty("color")]
        internal int ColorRaw { get; private set; }

        [JsonIgnore]
        public string Mention => string.Format(Consts.ROLE_MENTION, Id);

        [JsonIgnore]
        public Color Color => Color.FromArgb(ColorRaw);

        public override string ToString() => $"{Name} : {Id}";
    }
}