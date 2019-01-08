using Fractum.WebSocket;

namespace Fractum.WebSocket
{
    public abstract class CachedChannel : PopulatedEntity
    {
        protected internal CachedChannel(GatewayCache cache) : base(cache)
        {
        }

        public ChannelType Type { get; protected set; }
    }
}