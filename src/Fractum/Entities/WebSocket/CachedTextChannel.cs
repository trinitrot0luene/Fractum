using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities.Properties;
using Fractum.Entities.Rest;
using Fractum.Utilities;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;

namespace Fractum.Entities.WebSocket
{
    public sealed class CachedTextChannel : CachedGuildChannel, IMessageChannel
    {
        private VotedAsyncAction<ITextChannel> _typingAction;

        internal CachedTextChannel(FractumCache cache, ChannelCreateUpdateOrDeleteEventModel model,
            ulong? guildId = null) : base(cache, model, guildId)
        {
            Topic = model.Topic;
            LastPinAt = model.LastPinAt;
            PerUserRatelimit = model.PerUserRatelimit;
            LastMessageId = model.LastMessageId;
        }

        public string Topic { get; private set; }

        public DateTimeOffset? LastPinAt { get; private set; }

        public int PerUserRatelimit { get; private set; }

        public ulong? LastMessageId { get; private set; }

        public IEnumerable<CachedMessage> Messages => Cache[GuildId].GetMessages(Id);

        internal new void Update(ChannelCreateUpdateOrDeleteEventModel model)
        {
            base.Update(model);

            Topic = model.Topic;
            LastPinAt = model.LastPinAt;
            PerUserRatelimit = model.PerUserRatelimit;
            LastMessageId = model.LastMessageId;
        }

        #region REST

        public string Mention => string.Format(Consts.CHANNEL_MENTION, Id);

        public Task<RestMessage> CreateMessageAsync(string content = "", bool isTTS = false,
            EmbedBuilder embedBuilder = null, params (string fileName, Stream fileStream)[] attachments)
            => Client.RestClient.CreateMessageAsync(this.Id, content, isTTS, embedBuilder, attachments);

        public Task TriggerTypingAsync()
            => Client.RestClient.TriggerTypingAsync(Id);

        public Task<IMessage> GetMessageAsync(ulong messageId)
            => Client.GetMessage(this, messageId).GetAsync();

        public Task<IEnumerable<RestMessage>> GetMessagesAsync(int limit = 100)
            => Client.RestClient.GetMessagesAsync(this.Id, LastMessageId.Value, limit);

        public Task DeleteMessagesAsync(IEnumerable<IMessage> messages)
            => Client.RestClient.DeleteMessagesAsync(Id, messages.Select(m => m.Id));

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

            return await Client.RestClient.EditChannelAsync(Id, props) as RestTextChannel;
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

        #endregion
    }
}