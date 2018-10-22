using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.Entities.Rest;
using Fractum.Entities.WebSocket;
using Fractum.Utilities;

namespace Fractum.Contracts
{
    public interface IMessageChannel : IDiscordEntity
    {
        IEnumerable<CachedMessage> Messages { get; }

        Task<RestMessage> CreateMessageAsync(string content = "", bool isTTS = false, EmbedBuilder embedBuilder = null,
            params (string fileName, Stream fileStream)[] attachments);

        Task TriggerTypingAsync();

        Task<IMessage> GetMessageAsync(ulong messageId);

        Task<IEnumerable<RestMessage>> GetMessagesAsync(int limit = 100);

        DisposableScope<VotedAsyncAction<IMessageChannel>> BeginTyping();
    }
}