namespace Fractum.Contracts
{
    public interface IUser : IDiscordEntity
    {
        string Username { get; }

        short Discrim { get; }

        bool IsBot { get; }

        string Mention { get; }

        string GetAvatarUrl();
    }
}