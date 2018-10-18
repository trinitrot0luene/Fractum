using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.Utilities;
using Fractum.WebSocket;

namespace Fractum.Contracts
{
    public interface IMessageChannel : IDiscordEntity
    {
        Task<Message> CreateMessageAsync(string content = "", bool isTTS = false, EmbedBuilder embedBuilder = null,
            params (string fileName, Stream fileStream)[] attachments);

        ReadOnlyCollection<Message> Messages { get; }

        Task TriggerTypingAsync();

        Task<Message> GetMessageAsync(ulong messageId);

        Task<IReadOnlyCollection<Message>> GetMessagesAsync(int limit = 100);

        DisposableScope<VotedAsyncAction<IMessageChannel>> BeginTyping();
    }
}