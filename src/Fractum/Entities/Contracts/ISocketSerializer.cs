namespace Fractum.Entities.Contracts
{
    internal interface ISocketSerializer
    {
        string Deserialize();

        byte[] Serialize();
    }
}