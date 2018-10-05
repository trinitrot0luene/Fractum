using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Fractum.Entities
{
    public sealed class TextChannel : GuildChannel
    {
        [JsonProperty("topic")]
        public string Topic { get; private set; }

        [JsonProperty("last_pin_timestamp")]
        public DateTimeOffset? LastPinAt { get; private set; }

        [JsonProperty("last_message_id")]
        private string LastMessageIdRaw { get; set; }

        [JsonIgnore]
        public ulong? LastMessageId => LastMessageIdRaw is null ? default(ulong?) : ulong.Parse(LastMessageIdRaw);

        public Task<Message> CreateMessageAsync(EmbedBuilder EmbedBuilder)
            => Client.CreateMessageAsync(this, null, EmbedBuilder: EmbedBuilder);

        public Task<Message> CreateMessageAsync(string content, bool isTTS = false, EmbedBuilder EmbedBuilder = null)
            => Client.CreateMessageAsync(this, content, isTTS, EmbedBuilder);
    }
}