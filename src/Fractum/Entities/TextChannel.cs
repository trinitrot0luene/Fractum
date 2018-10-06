using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Fractum.Entities
{
    public sealed class TextChannel : GuildChannel
    {
        internal TextChannel()
        {
        }

        [JsonProperty("topic")]
        public string Topic { get; private set; }

        [JsonProperty("last_pin_timestamp")]
        public DateTimeOffset? LastPinAt { get; private set; }

        [JsonIgnore]
        public ReadOnlyCollection<Message> Messages { get; internal set; }

        [JsonProperty("last_message_id")]
        private string LastMessageIdRaw { get; set; }

        [JsonIgnore]
        public ulong? LastMessageId => LastMessageIdRaw is null ? default(ulong?) : ulong.Parse(LastMessageIdRaw);

        public Task<Message> CreateMessageAsync(EmbedBuilder embedBuilder)
            => Client.CreateMessageAsync(this, null, embedBuilder: embedBuilder);

        public Task<Message> CreateMessageAsync(string content, bool isTTS = false, EmbedBuilder embedBuilder = null)
            => Client.CreateMessageAsync(this, content, isTTS, embedBuilder);
    }
}