using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fractum.Rest;
using Fractum.WebSocket;
using Fractum.WebSocket.EventModels;
using Newtonsoft.Json;

namespace Fractum.WebSocket
{
    public class CachedDMChannel : CachedChannel, IMessageChannel
    {
        private VotedAsyncAction<ITextChannel> _typingAction;

        internal CachedDMChannel(FractumCache cache, ChannelCreateUpdateOrDeleteEventModel eventModel) : base(cache)
        {
            Id = eventModel.Id;
            Type = eventModel.Type;
            LastMessageId = eventModel.LastMessageId;
            Recipients = eventModel.Recipients;
        }

        [JsonProperty("last_message_id")]
        public ulong? LastMessageId { get; private set; }

        [JsonProperty("recipients")]
        public ReadOnlyCollection<User> Recipients { get; private set; }

        [JsonIgnore]
        public IEnumerable<CachedMessage> Messages { get; }

        [JsonIgnore]
        internal CircularBuffer<CachedMessage> MessageBuffer { get; set; }

        public Task<RestMessage> CreateMessageAsync(string content, bool isTTS = false,
            EmbedBuilder embedBuilder = null, params (string fileName, Stream fileStream)[] attachments)
            => Client.RestClient.CreateMessageAsync(this.Id, content, isTTS, embedBuilder, attachments);

        public Task TriggerTypingAsync()
            => Client.RestClient.TriggerTypingAsync(Id);

        public Task<IMessage> GetMessageAsync(ulong messageId)
        {
            if (Messages.FirstOrDefault(m => m.Id == messageId) is IMessage msg)
                return Task.FromResult(msg);
            else
                return Client.RestClient.GetMessageAsync(this.Id, messageId);
        }

        public Task<IEnumerable<RestMessage>> GetMessagesAsync(int limit = 100)
            => Client.RestClient.GetMessagesAsync(this.Id, LastMessageId.Value, limit);

        public DisposableScope<VotedAsyncAction<ITextChannel>> BeginTyping()
        {
            if (_typingAction == null)
                _typingAction =
                    new VotedAsyncAction<ITextChannel>(this, channel => channel.TriggerTypingAsync(), 9500);
            else
                _typingAction.Vote();
            return new DisposableScope<VotedAsyncAction<ITextChannel>>(_typingAction, x => x.Leave());
        }

        public Task<RestMessage> CreateMessageAsync(EmbedBuilder embedBuilder)
            => Client.RestClient.CreateMessageAsync(this.Id, null, embedBuilder: embedBuilder);
    }
}