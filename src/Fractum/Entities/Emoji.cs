using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fractum.Entities
{
    public sealed class Emoji : DiscordEntity
    {
        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("animated")]
        public bool IsAnimated { get; private set; }
    }
}
