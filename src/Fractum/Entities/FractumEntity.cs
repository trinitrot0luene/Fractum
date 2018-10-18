using Fractum.Rest;
using Fractum.WebSocket;

namespace Fractum.Entities
{
    public abstract class FractumEntity
    {
        internal FractumEntity()
        {
        }

        internal FractumSocketClient Client { get; private set; }

        internal FractumEntity WithClient(FractumSocketClient client)
        {
            Client = client;
            return this;
        }

        internal T WithClient<T>(FractumSocketClient client) where T : FractumEntity
        {
            Client = client;
            return this as T;
        }
    }
}