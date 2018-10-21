using Fractum.WebSocket.Core;

namespace Fractum.WebSocket.EventModels
{
    public abstract class EventModelBase
    {
        public T Cast<T>() where T : EventModelBase
            => (T) this;
    }
}