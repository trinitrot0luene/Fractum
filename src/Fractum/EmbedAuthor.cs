using Newtonsoft.Json;

namespace Fractum
{
    public sealed class EmbedAuthor
    {
        internal EmbedAuthor()
        {
        }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("icon_url")]
        public string IconUrl { get; set; }

        [JsonProperty("proxy_icon_url")]
        public string ProxyIconUrl { get; set; }
    }
}