using Fractum.WebSocket.EventModels;
using Newtonsoft.Json;

namespace Fractum.Entities.WebSocket
{
    /// <summary>
    ///     Generated when a user reacts or removes a reaction from a message.
    /// </summary>
    public sealed class CachedReaction
    {
        internal CachedReaction(ReactionAddEventModel model)
        {
            UserId = model.UserId;
            ChannelId = model.ChannelId;
            GuildId = model.GuildId;
            Emoji = model.Emoji;
        }

        internal CachedReaction(ReactionRemoveEventModel model)
        {
            UserId = model.UserId;
            ChannelId = model.ChannelId;
            GuildId = model.GuildId;
            Emoji = model.Emoji;
        }

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