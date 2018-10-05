using Fractum.Rest;

namespace Fractum.Entities
{
    public abstract class FractumEntity
    {
        internal FractumEntity()
        {
        }

        internal FractumRestClient Client { get; private set; }

        internal FractumEntity WithClient(FractumRestClient restClient)
        {
            Client = restClient;
            return this;
        }

        internal T WithClient<T>(FractumRestClient restClient) where T : FractumEntity
        {
            Client = restClient;
            return this as T;
        }
    }
}