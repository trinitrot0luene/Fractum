using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities.Rest;
using Fractum.Utilities;
using Fractum.WebSocket.Core;
using Newtonsoft.Json;

namespace Fractum.Entities.WebSocket
{
    public class CachedDMChannel : CachedChannel, IMessageChannel
    {
        private VotedAsyncAction<IMessageChannel> _typingAction;

        internal CachedDMChannel(FractumCache cache) : base(cache)
        {
        }

        [JsonProperty("last_message_id")]
        public ulong? LastMessageId { get; private set; }

        [JsonProperty("recipients")]
        public ReadOnlyCollection<User> Recipients { get; private set; }

        [JsonIgnore]
        public IEnumerable<CachedMessage> Messages { get; internal set; }

        public Task<RestMessage> CreateMessageAsync(string content, bool isTTS = false,
            EmbedBuilder embedBuilder = null, params (string fileName, Stream fileStream)[] attachments)
            => Client.RestClient.CreateMessageAsync(this, content, isTTS, embedBuilder, attachments);

        public Task TriggerTypingAsync()
            => Client.RestClient.TriggerTypingAsync(Id);

        public async Task<IMessage> GetMessageAsync(ulong messageId)
        {
            var message = await Client.GetMessage(this, messageId).GetAsync();

            return message;
        }

        public Task<IEnumerable<RestMessage>> GetMessagesAsync(int limit = 100)
            => Client.RestClient.GetMessagesAsync(this, LastMessageId.Value, limit);

        public DisposableScope<VotedAsyncAction<IMessageChannel>> BeginTyping()
        {
            if (_typingAction == null)
                _typingAction =
                    new VotedAsyncAction<IMessageChannel>(this, channel => channel.TriggerTypingAsync(), 9500);
            else
                _typingAction.Vote();
            return new DisposableScope<VotedAsyncAction<IMessageChannel>>(_typingAction, x => x.Leave());
        }

        public Task<RestMessage> CreateMessageAsync(EmbedBuilder embedBuilder)
            => Client.RestClient.CreateMessageAsync(this, null, embedBuilder: embedBuilder);
    }
}