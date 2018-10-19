using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities.Extensions;
using Fractum.Entities.Properties;
using Fractum.Utilities;
using Fractum.WebSocket;
using Fractum.WebSocket.Core;
using Newtonsoft.Json;

namespace Fractum.Entities
{
    public sealed class TextChannel : GuildChannel, IMessageChannel
    {
        private VotedAsyncAction<IMessageChannel> _typingAction;

        internal TextChannel()
        {
        }

        [JsonProperty("topic")]
        private string TopicRaw
        {
            set => Topic = value ?? string.Empty;
        }

        [JsonIgnore]
        public string Topic { get; private set; }

        [JsonProperty("last_pin_timestamp")]
        public DateTimeOffset? LastPinAt { get; private set; }

        [JsonProperty("rate_limit_per_user")]
        public int PerUserRatelimit { get; private set; }

        [JsonProperty("last_message_id")]
        public ulong? LastMessageId { get; internal set; }

        [JsonIgnore]
        public IReadOnlyCollection<Message> Messages => Cache.GetMessages(Id);

        [JsonIgnore]
        public string Mention => string.Format(Consts.CHANNEL_MENTION, Id);

        public Task<Message> CreateMessageAsync(string content = "", bool isTTS = false, EmbedBuilder embedBuilder = null, params (string fileName, Stream fileStream)[] attachments)
            => Client.CreateMessageAsync(this, content, isTTS, embedBuilder, attachments);

        public Task TriggerTypingAsync()
            => Client.RestClient.TriggerTypingAsync(Id);

        public Task<Message> GetMessageAsync(ulong messageId)
            => Client.GetMessage(this, messageId).GetAsync();

        public Task<IReadOnlyCollection<Message>> GetMessagesAsync(int limit = 100)
            => Client.GetMessagesAsync(this, LastMessageId.Value, limit);

        public Task DeleteMessagesAsync(IEnumerable<Message> messages)
            => Client.RestClient.DeleteMessagesAsync(Id, messages.Select(m => m.Id));

        public async Task<TextChannel> EditAsync(Action<TextChannelProperties> editAction)
        {
            var props = new TextChannelProperties()
            {
                IsNsfw = this.IsNsfw,
                Name = this.Name,
                ParentId = this.ParentId,
                PermissionsOverwrites = this.Overwrites,
                PerUserRatelimit = this.PerUserRatelimit,
                Position = this.Position,
                Topic = this.Topic
            };
            editAction(props);

            return await Client.EditChannelAsync(Id, props) as TextChannel;
        }

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