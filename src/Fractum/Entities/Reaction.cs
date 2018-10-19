using Newtonsoft.Json;

namespace Fractum.Entities
{
    /// <summary>
    ///     Generated when a user reacts or removes a reaction from a message.
    /// </summary>
    public sealed class Reaction
    {
        [JsonProperty("user_id")]
        public ulong UserId { get; private set; }

        [JsonProperty("channel_id")]
        public ulong ChannelId { get; private set; }

        [JsonProperty("guild_id")]
        public ulong? GuildId { get; private set; }

        [JsonProperty("emoji")]
        public Emoji Emoji { get; private set; }
    }
}