using Fractum.WebSocket;

namespace Fractum.Entities
{
    public abstract class PopulatedEntity : DiscordEntity
    {
        protected PopulatedEntity(ISocketCache<ISyncedGuild> cache)
        {
            Cache = cache;
        }

        internal ISocketCache<ISyncedGuild> Cache { get; }

        internal FractumSocketClient Client => Cache.Client;
    }
}