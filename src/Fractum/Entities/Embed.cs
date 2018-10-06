using System;
using Newtonsoft.Json;

namespace Fractum.Entities
{
    public sealed class Embed
    {
        internal Embed()
        {
        }

        [JsonProperty("title")]
        public string Title { get; internal set; }

        [JsonProperty("type")]
        public string Type { get; internal set; }

        [JsonProperty("description")]
        public string Description { get; internal set; }

        [JsonProperty("url")]
        public string Url { get; internal set; }

        [JsonProperty("timestamp")]
        public DateTimeOffset? Timestamp { get; internal set; }

        [JsonProperty("color")]
        public int Color { get; internal set; }

        [JsonProperty("footer")]
        public EmbedFooter Footer { get; internal set; }

        [JsonProperty("thumbnail")]
        public EmbedContent Thumbnail { get; internal set; }

        [JsonProperty("video")]
        public EmbedContent Video { get; internal set; }

        [JsonProperty("image")]
        public EmbedContent Image { get; internal set; }

        [JsonProperty("author")]
        public EmbedAuthor Author { get; internal set; }

        [JsonProperty("fields")]
        public EmbedField[] Fields { get; internal set; }
    }
}