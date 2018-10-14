using Newtonsoft.Json;

namespace Fractum.Entities.Properties
{
    public class MessageEditProperties
    {
        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("embed")]
        public Embed Embed { get; set; }
    }
}