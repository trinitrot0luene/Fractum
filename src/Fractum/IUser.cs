namespace Fractum
{
    public interface IUser : IDiscordEntity
    {
        string Username { get; }

        short DiscrimValue { get; }

        bool IsBot { get; }

        string Mention { get; }

        string GetAvatarUrl();
    }
}