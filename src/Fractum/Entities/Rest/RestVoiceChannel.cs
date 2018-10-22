using Newtonsoft.Json;

namespace Fractum.Entities.Rest
{
    public class RestVoiceChannel : RestGuildChannel
    {
        internal RestVoiceChannel()
        {
        }

        [JsonProperty("user_limit")]
        public int UserLimit { get; private set; }

        [JsonProperty("bitrate")]
        public int Bitrate { get; private set; }
    }
}