using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.Entities.Rest;
using Fractum.Utilities;

namespace Fractum.Contracts
{
    public interface ITextChannel : IDiscordEntity
    {
        Task<RestMessage> CreateMessageAsync(string content = "", bool isTTS = false, EmbedBuilder embedBuilder = null,
            params (string fileName, Stream fileStream)[] attachments);

        Task TriggerTypingAsync();

        Task<IMessage> GetMessageAsync(ulong messageId);

        Task<IEnumerable<RestMessage>> GetMessagesAsync(int limit = 100);

        DisposableScope<VotedAsyncAction<ITextChannel>> BeginTyping();
    }
}