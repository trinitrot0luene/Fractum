using Newtonsoft.Json;

namespace Fractum.Entities
{
    public sealed class EmbedField
    {
        internal EmbedField() { }

        [JsonProperty("name")]
        public string Name { get; internal set; }

        [JsonProperty("value")]
        public string Value { get; internal set; }

        [JsonProperty("inline")]
        public bool IsInline { get; internal set; }
    }
}