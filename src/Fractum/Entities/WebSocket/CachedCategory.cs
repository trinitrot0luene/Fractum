using Fractum.WebSocket;
using Fractum.WebSocket.EventModels;

namespace Fractum.Entities.WebSocket
{
    public sealed class CachedCategory : CachedGuildChannel
    {
        internal CachedCategory(ISocketCache<ISyncedGuild> cache, ChannelCreateUpdateOrDeleteEventModel model, ulong? guildId = null)
            : base(cache, model, guildId)
        {
        }
    }
}