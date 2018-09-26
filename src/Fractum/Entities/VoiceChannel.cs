using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fractum.Entities
{
    public sealed class VoiceChannel : GuildChannel
    {
        [JsonProperty("user_limit")]
        public int UserLimit { get; private set; }

        [JsonProperty("bitrate")]
        public int Bitrate { get; private set; }
    }
}
