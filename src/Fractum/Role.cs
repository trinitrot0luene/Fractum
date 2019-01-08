using System.Drawing;
using Newtonsoft.Json;

namespace Fractum
{
    public sealed class Role : DiscordEntity
    {
        public Role()
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
        public int ColorValue { get; private set; }

        [JsonIgnore]
        public string Mention => string.Format(Consts.ROLE_MENTION, Id);

        [JsonIgnore]
        public Color Color => Color.FromArgb(ColorValue);

        public override string ToString() => $"{Name} : {Id}";
    }
}