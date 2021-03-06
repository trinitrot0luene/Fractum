﻿using Newtonsoft.Json;

namespace Fractum
{
    public sealed class Attachment : DiscordEntity
    {
        internal Attachment()
        {
        }

        [JsonProperty("filename")]
        public string Filename { get; private set; }

        [JsonProperty("size")]
        public int Filesize { get; private set; }

        [JsonProperty("url")]
        public string Url { get; private set; }

        [JsonProperty("proxy_url")]
        public string ProxyUrl { get; private set; }

        [JsonProperty("height")]
        public int? Height { get; private set; }

        [JsonProperty("width")]
        public int? Width { get; private set; }
    }
}