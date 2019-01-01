using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Fractum;
using Fractum.Rest;
using Fractum.Extensions;
using Fractum.Rest.Utils;
using Fractum.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fractum.Rest
{
    public sealed class FractumRestClient
    {
        internal FractumConfig Config;

        internal FractumRestService RestService;

        public FractumRestClient(FractumConfig config)
        {
            Config = config;

            RestService = new FractumRestService(config);

            RestService.OnLog += msg =>
            {
                InvokeLog(msg);
                return Task.CompletedTask;
            };
        }

        #region /audit-logs

        #endregion

        #region /channels

        public async Task<IGuildChannel> GetChannelAsync(ulong channelId)
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.CHANNELS), channelId);
            var response = await RestService.SendRequestAsync(new StringRestRequest(rb, HttpMethod.Get))
                .ConfigureAwait(false);

            var channelObject = JObject.Parse(await response.Content.ReadAsStringAsync());

            var channelType = (ChannelType)channelObject.Value<int>("type");
            switch (channelType)
            {
                case ChannelType.GuildText:
                    var textChannel = channelObject.ToObject<RestTextChannel>();
                    textChannel.Client = this;
                    return textChannel;
                case ChannelType.GuildVoice:
                    var voiceChannel = channelObject.ToObject<RestVoiceChannel>();
                    voiceChannel.Client = this;
                    return voiceChannel;
                case ChannelType.GuildCategory:
                    var category = channelObject.ToObject<RestCategory>();
                    category.Client = this;
                    return category;
                default:
                    return default;
            }
        }

        public async Task<RestGuildChannel> EditChannelAsync(ulong channelId, GuildChannelProperties props)
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.CHANNELS, true), channelId);

            var response = await RestService.SendRequestAsync(new StringRestRequest(rb, HttpMethod.Put, channelId, props.Serialize()))
                .ConfigureAwait(false);

            var channelObject = JObject.Parse(await response.Content.ReadAsStringAsync());
            var channelType = (ChannelType)channelObject.Value<int>("type");
            switch (channelType)
            {
                case ChannelType.GuildText:
                    var textChannel = channelObject.ToObject<RestTextChannel>();
                    textChannel.Client = this;
                    return textChannel;
                case ChannelType.GuildVoice:
                    var voiceChannel = channelObject.ToObject<RestVoiceChannel>();
                    voiceChannel.Client = this;
                    return voiceChannel;
                case ChannelType.GuildCategory:
                    var category = channelObject.ToObject<RestCategory>();
                    category.Client = this;
                    return category;
                default:
                    return default;
            }
        }

        public async Task DeleteChannelAsync(ulong channelId)
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.CHANNELS, true), channelId);

            _ = await RestService.SendRequestAsync(new StringRestRequest(rb, HttpMethod.Delete, channelId))
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<RestMessage>> GetMessagesAsync(ulong channelId, ulong messageId,
            int target)
        {
            var messages = new List<RestMessage>();
            do
            {
                var limit = target < 100 ? target : 100;
                target -= limit;

                var rb = new RouteBuilder()
                    .WithPart(RouteSection.Create(Consts.CHANNELS, true), channelId)
                    .WithPart(RouteSection.Create(Consts.MESSAGES))
                    .WithParameterBuilder(new RouteParameterBuilder()
                        .Add("before", messageId.ToString())
                        .Add("limit", limit.ToString()));

                var resp = await RestService.SendRequestAsync(new StringRestRequest(rb, HttpMethod.Get, channelId))
                    .ConfigureAwait(false);

                var responseContent = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                var msgs = responseContent.Deserialize<RestMessage[]>();
                foreach (var msg in msgs)
                    msg.Client = this;
                messages.AddRange(msgs);

                messageId = messages[messages.Count - 1].Id;
            } while (target > 0);

            return messages;
        }

        public async Task<IMessage> GetMessageAsync(ulong channelId, ulong messageId)
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.CHANNELS, true), channelId)
                .WithPart(RouteSection.Create(Consts.MESSAGES), messageId);

            var resp = await RestService.SendRequestAsync(new StringRestRequest(rb, HttpMethod.Get, channelId))
                .ConfigureAwait(false);

            return await resp.Content.ReadAsObjectAsync<RestMessage>(this);
        }

        public Task TriggerTypingAsync(ulong channelId)
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.CHANNELS, true), channelId)
                .WithPart(RouteSection.Create(Consts.TYPING));
            return RestService.SendRequestAsync(new StringRestRequest(rb, HttpMethod.Post, channelId));
        }

        public async Task<RestMessage> CreateMessageAsync(ulong channelId, string content, bool isTTS = false,
            EmbedBuilder embedBuilder = null, params (string fileName, Stream fileStream)[] attachments)
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.CHANNELS, true), channelId)
                .WithPart(RouteSection.Create(Consts.MESSAGES));

            var multipartFields = new Dictionary<string, string>();
            var multipartFiles = new Dictionary<string, (string, Stream)>();

            multipartFields.Add("tts", isTTS.ToString().ToLower());
            if (content != null)
                multipartFields.Add("content", content);
            if (embedBuilder != null)
                multipartFields.Add("payload_json", new
                {
                    embed = embedBuilder.Create()
                }.Serialize());

            foreach (var item in attachments)
                multipartFiles.Add("file", item);

            var response = await RestService.SendRequestAsync(new MultipartRestRequest(rb, HttpMethod.Post, multipartFields,
                multipartFiles, channelId)).ConfigureAwait(false);

            return await response.Content.ReadAsObjectAsync<RestMessage>(this);
        }

        public async Task<RestMessage> EditMessageAsync(ulong messageId, ulong channelId, MessageEditProperties props)
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.CHANNELS, true), channelId)
                .WithPart(RouteSection.Create(Consts.MESSAGES), messageId);

            var response = await RestService.SendRequestAsync(new StringRestRequest(rb, new HttpMethod("PATCH"),
                channelId, props.Serialize()));

            return await response.Content.ReadAsObjectAsync<RestMessage>(this);
        }

        public Task CreateReactionAsync(ulong messageId, ulong channelId, Emoji emoji)
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.CHANNELS, true), channelId)
                .WithPart(RouteSection.Create(Consts.MESSAGES), messageId)
                .WithPart(RouteSection.Create(Consts.REACTIONS), emoji.ToString())
                .WithPart(RouteSection.Create(Consts.BLANK), Consts.ME);

            return RestService.SendRequestAsync(new StringRestRequest(rb, HttpMethod.Put, channelId));
        }

        public Task DeleteReactionAsync(ulong messageId, ulong channelId, Emoji emoji, ulong? userId = null)
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.CHANNELS, true), channelId)
                .WithPart(RouteSection.Create(Consts.MESSAGES), messageId)
                .WithPart(RouteSection.Create(Consts.REACTIONS), emoji.ToString())
                .WithPart(RouteSection.Create(Consts.BLANK), userId == null ? userId.ToString() : Consts.ME);

            return RestService.SendRequestAsync(new StringRestRequest(rb, HttpMethod.Delete, channelId));
        }

        public async Task<IReadOnlyCollection<RestUser>> GetReactionsAsync(ulong messageId, ulong channelId, Emoji emoji,
            int limit = 25)
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.CHANNELS, true), channelId)
                .WithPart(RouteSection.Create(Consts.MESSAGES), messageId)
                .WithPart(RouteSection.Create(Consts.REACTIONS), emoji.ToString())
                .WithParameterBuilder(new RouteParameterBuilder()
                    .Add("limit", limit.ToString()));

            var response = await RestService
                .SendRequestAsync(new StringRestRequest(rb, HttpMethod.Get, channelId)).ConfigureAwait(false);

            return await response.Content.ReadAsObjectAsync<ReadOnlyCollection<RestUser>>();
        }

        public Task ClearReactionsAsync(ulong messageId, ulong channelId)
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.CHANNELS, true), channelId)
                .WithPart(RouteSection.Create(Consts.MESSAGES), messageId)
                .WithPart(RouteSection.Create(Consts.REACTIONS), string.Empty);

            return RestService.SendRequestAsync(new StringRestRequest(rb, HttpMethod.Delete, channelId));
        }

        public Task DeleteMessageAsync(ulong messageId, ulong channelId)
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.CHANNELS, true), channelId)
                .WithPart(RouteSection.Create(Consts.MESSAGES), messageId);

            return RestService.SendRequestAsync(new StringRestRequest(rb, HttpMethod.Delete,
                channelId));
        }

        public Task DeleteMessagesAsync(ulong channelId, IEnumerable<ulong> messageIds)
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.CHANNELS, true), channelId)
                .WithPart(RouteSection.Create(Consts.MESSAGES))
                .WithPart(RouteSection.Create(Consts.BULK_DELETE));

            return RestService.SendRequestAsync(new StringRestRequest(rb, HttpMethod.Post,
                channelId, new { messages = messageIds }.Serialize()));
        }

        #endregion

        #region /emojis

        #endregion

        #region /guilds

        #endregion

        #region /invites

        #endregion

        #region /users

        public Task LeaveGuildAsync(ulong guildId)
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.USERS), Consts.ME)
                .WithPart(RouteSection.Create(Consts.GUILDS), guildId);

            return RestService.SendRequestAsync(new StringRestRequest(rb, HttpMethod.Delete));
        }

        public async Task<RestDMChannel> CreateDMChannelAsync(ulong userId)
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.USERS), Consts.ME)
                .WithPart(RouteSection.Create(Consts.CHANNELS), string.Empty);

            var response = await RestService.SendRequestAsync(new StringRestRequest(rb, HttpMethod.Post, 0, new { recipient_id = userId }.Serialize()));

            return await response.Content.ReadAsObjectAsync<RestDMChannel>(this);
        }

        public async Task<RestUser> GetUserAsync(ulong userId)
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.USERS), userId);

            var response = await RestService.SendRequestAsync(new StringRestRequest(rb, HttpMethod.Get));

            return await response.Content.ReadAsObjectAsync<RestUser>(this);
        }

        #endregion

        public async Task<GatewayDetails> GetSocketUrlAsync()
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.GATEWAY))
                .WithPart(RouteSection.Create(Consts.BOT));

            var resp = await RestService.SendRequestAsync(new StringRestRequest(rb, HttpMethod.Get))
                .ConfigureAwait(false);

            return JsonConvert.DeserializeObject<GatewayDetails>(await resp.Content.ReadAsStringAsync()
                .ConfigureAwait(false));
        }

        internal void InvokeLog(LogMessage msg)
            => OnLog?.Invoke(msg);

        /// <summary>
        ///     Raised when the client receives a log event.
        /// </summary>
        public event Func<LogMessage, Task> OnLog;
    }
}