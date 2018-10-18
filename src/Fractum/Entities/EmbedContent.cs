using Newtonsoft.Json;

namespace Fractum.Entities
{
    public sealed class EmbedContent
    {
        [JsonProperty("url")]
        public string Url { get; internal set; }

        [JsonProperty("proxy_url", NullValueHandling = NullValueHandling.Ignore)]
        public string ProxiedUrl { get; internal set; }

        [JsonProperty("height")]
        public int Height { get; internal set; }

        [JsonProperty("width")]
        public int Width { get; internal set; }
    }
}