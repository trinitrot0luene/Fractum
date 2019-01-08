using Fractum;
using Fractum.Rest;
using Fractum.WebSocket;
using Qmmands;
using System.IO;
using System.Threading.Tasks;

namespace Fractum.Testing
{
    public sealed class CommandContext : ICommandContext
    {
        public CommandContext(GatewayClient client, CachedMessage message)
        {
            Client = client;
            Guild = (message.Channel as CachedGuildChannel)?.Guild;
            Channel = message.Channel;
            User = message.Author;
        }

        public GatewayClient Client { get; }

        public CachedGuild Guild { get; }

        public IMessageChannel Channel { get; }

        public IUser User { get; }

        public CachedMember Member => User as CachedMember;

        public Task<RestMessage> RespondAsync(string content, bool isTTS = false, EmbedBuilder embedBuilder = null, 
            params (string, Stream)[] attachments)
            => Channel.CreateMessageAsync(content, isTTS, embedBuilder, attachments);
    }
}
