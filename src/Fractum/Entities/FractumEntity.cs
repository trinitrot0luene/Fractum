using Fractum.Rest;
using Fractum.WebSocket;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;

namespace Fractum.Entities
{
    public abstract class FractumEntity
    {
        internal FractumEntity(EventModelBase model, FractumCache cache)
        {
            Cache = cache;
        }

        internal FractumCache Cache { get; }

        internal FractumSocketClient Client => Cache.Client;

        internal abstract void ApplyModel();
    }
}