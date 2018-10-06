using Fractum.Rest;

namespace Fractum.Entities
{
    public abstract class FractumEntity
    {
        internal FractumEntity()
        {
        }

        internal FractumRestClient Client { get; private set; }

        internal FractumEntity WithClient(FractumRestClient client)
        {
            Client = client;
            return this;
        }

        internal T WithClient<T>(FractumRestClient client) where T : FractumEntity
        {
            Client = client;
            return this as T;
        }
    }
}