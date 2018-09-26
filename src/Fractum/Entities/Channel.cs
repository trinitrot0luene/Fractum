using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fractum.Entities
{
    public class Channel : DiscordEntity
    {
        [JsonProperty("type")]
        public ChannelType Type { get; private set; }

        [JsonProperty("name")]
        public string Name { get; private set; }
    }
}
