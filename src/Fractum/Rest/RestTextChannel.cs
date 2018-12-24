using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Fractum.Rest
{
    public class RestTextChannel : RestGuildChannel, ITextChannel
    {
        private VotedAsyncAction<ITextChannel> _typingAction;

        internal RestTextChannel()
        {
        }

        [JsonProperty("nsfw")]
        public bool IsNsfw { get; private set; }

        [JsonProperty("topic")]
        public string Topic { get; private set; }

        [JsonProperty("last_pin_timestamp")]
        public DateTimeOffset? LastPinAt { get; private set; }

        [JsonProperty("rate_limit_per_user")]
        public int PerUserRatelimit { get; private set; }

        [JsonProperty("last_message_id")]
        public ulong? LastMessageId { get; private set; }

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

        public async Task<RestTextChannel> EditAsync(Action<TextChannelProperties> editAction)
        {
            var props = new TextChannelProperties
            {
                IsNsfw = IsNsfw,
                Name = Name,
                ParentId = ParentId,
                PermissionsOverwrites = Overwrites,
                PerUserRatelimit = PerUserRatelimit,
                Position = Position,
                Topic = Topic
            };
            editAction(props);

            return await Client.EditChannelAsync(Id, props) as RestTextChannel;
        }

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