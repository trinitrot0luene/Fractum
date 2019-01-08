using Newtonsoft.Json;

namespace Fractum.Rest
{
    public class VoiceRegion
    {
        internal VoiceRegion()
        {
        }

        [JsonProperty("id")]
        public string Id { get; private set; }

        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("vip")]
        public bool IsVip { get; private set; }

        [JsonProperty("optimal")]
        public bool IsOptimal { get; private set; }

        [JsonProperty("deprecated")]
        public bool IsDeprecated { get; private set; }

        [JsonProperty("custom")]
        public bool IsCustom { get; private set; }
    }
}