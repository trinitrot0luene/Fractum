using Fractum.WebSocket.Pipelines;

namespace Fractum.WebSocket.Events
{
    internal interface IEvent
    {
        void ApplyToCache(FractumCache cache);
    }
}