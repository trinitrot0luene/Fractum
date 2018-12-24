using Newtonsoft.Json;

namespace Fractum
{
    public sealed class EmbedFooter
    {
        internal EmbedFooter()
        {
        }

        [JsonProperty("text")]
        public string Text { get; internal set; }

        [JsonProperty("icon_url")]
        public string IconUrl { get; internal set; }

        [JsonProperty("proxy_icon_url")]
        public string ProxyIconUrl { get; internal set; }
    }
}