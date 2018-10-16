using Fractum.Entities;
using Fractum.WebSocket.Core;

namespace Fractum.WebSocket.EventModels
{
    /// <summary>
    ///     Models event payloads to be applied to cache.
    /// </summary>
    internal abstract class BaseEventModel : DiscordEntity
    {
        public abstract void ApplyToCache(FractumCache cache);
    }
}