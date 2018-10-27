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
            MessageId = model.MessageId;
            GuildId = model.GuildId;
            Emoji = model.Emoji;
        }

        internal CachedReaction(ReactionRemoveEventModel model)
        {
            UserId = model.UserId;
            ChannelId = model.ChannelId;
            MessageId = model.MessageId;
            GuildId = model.GuildId;
            Emoji = model.Emoji;
        }

        public ulong UserId { get; }

        public ulong ChannelId { get; }

        public ulong MessageId { get; }

        public ulong? GuildId { get; }

        public Emoji Emoji { get; }
    }
}