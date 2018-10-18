using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Utilities;
using Fractum.WebSocket;
using Newtonsoft.Json;

namespace Fractum.Entities
{
    public class PrivateChannel : Channel, IMessageChannel
    {
        private VotedAsyncAction<IMessageChannel> _typingAction;

        internal PrivateChannel()
        {
        }

        [JsonProperty("last_message_id")]
        public ulong? LastMessageId { get; private set; }

        [JsonProperty("recipients")]
        public ReadOnlyCollection<IUser> Recipients { get; private set; }

        [JsonIgnore]
        public ReadOnlyCollection<Message> Messages { get; internal set; }

        public Task<Message> CreateMessageAsync(EmbedBuilder embedBuilder)
            => Client.CreateMessageAsync(this, null, embedBuilder: embedBuilder);

        public Task<Message> CreateMessageAsync(string content, bool isTTS = false, EmbedBuilder embedBuilder = null, params (string fileName, Stream fileStream)[] attachments)
            => Client.CreateMessageAsync(this, content, isTTS, embedBuilder, attachments);

        public Task TriggerTypingAsync()
            => Client.RestClient.TriggerTypingAsync(Id);

        public async Task<Message> GetMessageAsync(ulong messageId)
        {
            var message = await Client.GetMessage(this, messageId).GetAsync();

            return message;
        }

        public Task<IReadOnlyCollection<Message>> GetMessagesAsync(int limit = 100)
            => Client.RestClient.GetMessagesAsync(this, LastMessageId.Value, limit);

        public DisposableScope<VotedAsyncAction<IMessageChannel>> BeginTyping()
        {
            if (_typingAction == null)
                _typingAction = new VotedAsyncAction<IMessageChannel>(this, channel => channel.TriggerTypingAsync(), 9500);
            else
                _typingAction.Vote();
            return new DisposableScope<VotedAsyncAction<IMessageChannel>>(_typingAction, x => x.Leave());
        }
    }
}