using Fractum.WebSocket;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket
{
    public sealed class CachedCategory : CachedGuildChannel
    {
        internal CachedCategory(FractumCache cache, ChannelCreateUpdateOrDeleteEventModel model, ulong? guildId = null)
            : base(cache, model, guildId)
        {
        }
    }
}