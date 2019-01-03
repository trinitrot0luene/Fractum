using System.Threading.Tasks;

namespace Fractum
{
    public interface IGuild
    {
        int AfkTimeout { get; }

        GuildEmoji[] Emojis { get; }

        ExplicitContentFilterLevel ExplicitContentFilterLevel { get; }

        MessageNotificationLevel MessageNotificationLevel { get; }

        string Name { get; }

        ulong OwnerId { get; }

        string Region { get; }

        Role[] Roles { get; }

        VerificationLevel VerificationLevel { get; }

        Task DeleteAsync();

        string GetIconUrl();

        string GetSplashUrl();
    }
}
