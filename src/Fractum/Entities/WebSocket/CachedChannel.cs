using Fractum.WebSocket.Core;

namespace Fractum.Entities
{
    public abstract class CachedChannel : PopulatedEntity
    {
        protected internal CachedChannel(FractumCache cache) : base(cache)
        {
        }

        public ChannelType Type { get; protected set; }
    }
}