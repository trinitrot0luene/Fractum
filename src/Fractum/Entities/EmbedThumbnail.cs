using Newtonsoft.Json;

namespace Fractum.Entities
{
    public sealed class EmbedThumbnail
    {
        internal EmbedThumbnail()
        {
        }

        [JsonProperty("url")]
        public string Url { get; internal set; }

        [JsonProperty("proxy_url")]
        public string ProxiedUrl { get; internal set; }

        [JsonProperty("height")]
        public int Height { get; internal set; }

        [JsonProperty("width")]
        public int Width { get; internal set; }
    }
}