using Fractum.Entities;
using Fractum.WebSocket.Core;

namespace Fractum.WebSocket.EventModels
{
    /// <summary>
    ///     Models event payloads to be applied to cache.
    /// </summary>
    public abstract class BaseEventModel : DiscordEntity
    {
        public abstract void ApplyToCache(FractumCache cache);
    }
}