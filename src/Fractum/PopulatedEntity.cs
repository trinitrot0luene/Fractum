using Fractum.WebSocket;

namespace Fractum
{
    public abstract class PopulatedEntity : DiscordEntity
    {
        protected PopulatedEntity(GatewayCache cache)
        {
            Cache = cache;
        }

        internal GatewayCache Cache { get; }

        internal GatewayClient Client => Cache.Client;
    }
}
