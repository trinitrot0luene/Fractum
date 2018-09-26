using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Fractum.Entities
{
    public sealed class Role : DiscordEntity
    {
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

        [JsonIgnore]
        public Color Color { get => Color.FromArgb(ColorRaw); }

        [JsonProperty("color")]
        private int ColorRaw { get; set; }
    }
}
