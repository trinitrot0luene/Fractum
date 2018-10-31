using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fractum.Entities.Rest
{
    public sealed class RestDMChannel : RestChannel, ITextChannel
    {
        private VotedAsyncAction<ITextChannel> _typingAction;

        internal RestDMChannel()
        {
        }

        [JsonProperty("last_message_id")]
        public ulong? LastMessageId { get; private set; }

        [JsonProperty("recipients")]
        public User[] Users { get; private set; }

        public Task<RestMessage> CreateMessageAsync(string content = "", bool isTTS = false,
            EmbedBuilder embedBuilder = null, params (string fileName, Stream fileStream)[] attachments)
            => Client.CreateMessageAsync(this.Id, content, isTTS, embedBuilder, attachments);

        public Task TriggerTypingAsync()
            => Client.TriggerTypingAsync(Id);

        public Task<IMessage> GetMessageAsync(ulong messageId)
            => Client.GetMessageAsync(this.Id, messageId);

        public Task<IEnumerable<RestMessage>> GetMessagesAsync(int limit = 100)
            => Client.GetMessagesAsync(this.Id, LastMessageId ?? 0, limit);

        public Task DeleteMessagesAsync(IEnumerable<IMessage> messages)
            => Client.DeleteMessagesAsync(Id, messages.Select(m => m.Id));

        public DisposableScope<VotedAsyncAction<ITextChannel>> BeginTyping()
        {
            if (_typingAction == null)
                _typingAction =
                    new VotedAsyncAction<ITextChannel>(this, channel => channel.TriggerTypingAsync(), 9500);
            else
                _typingAction.Vote();
            return new DisposableScope<VotedAsyncAction<ITextChannel>>(_typingAction, x => x.Leave());
        }
    }
}
