using Fractum.WebSocket;

namespace Fractum.Entities.WebSocket
{
    public abstract class CachedChannel : PopulatedEntity
    {
        protected internal CachedChannel(ISocketCache<ISyncedGuild> cache) : base(cache)
        {
        }

        public ChannelType Type { get; protected set; }
    }
}