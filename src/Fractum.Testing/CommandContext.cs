using Fractum.Entities;
using Fractum.Entities.Rest;
using Fractum.Entities.WebSocket;
using Fractum.WebSocket;
using Qmmands;
using System.IO;
using System.Threading.Tasks;

namespace Fractum.Testing
{
    public sealed class CommandContext : ICommandContext
    {
        public CommandContext(FractumSocketClient client, CachedMessage message)
        {
            Client = client;
            Guild = (message.Channel as CachedGuildChannel)?.Guild;
            Channel = message.Channel;
            User = message.Author;
        }

        FractumSocketClient Client { get; }

        CachedGuild Guild { get; }

        IMessageChannel Channel { get; }

        IUser User { get; }

        CachedMember Member => User as CachedMember;

        public Task<RestMessage> RespondAsync(string content, bool isTTS = false, EmbedBuilder embedBuilder = null, 
            params (string, Stream)[] attachments)
            => Channel.CreateMessageAsync(content, isTTS, embedBuilder, attachments);
    }
}
