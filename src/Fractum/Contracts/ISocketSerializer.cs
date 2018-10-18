namespace Fractum.Contracts
{
    internal interface ISocketSerializer
    {
        string Deserialize();

        byte[] Serialize();
    }
}