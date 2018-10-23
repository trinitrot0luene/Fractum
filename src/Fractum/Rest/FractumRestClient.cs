using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.Entities.Extensions;
using Fractum.Entities.Properties;
using Fractum.Entities.Rest;
using Fractum.Entities.WebSocket;
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

            RestService.Log += msg =>
            {
                InvokeLog(msg);
                return Task.CompletedTask;
            };
        }

        public Task TriggerTypingAsync(ulong channelId)
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.CHANNELS, true), channelId)
                .WithPart(RouteSection.Create(Consts.TYPING));
            return RestService.SendRequestAsync(new StringRestRequest(rb, HttpMethod.Post, channelId));
        }

        public async Task<IGuildChannel> GetChannelAsync(ulong channelId)
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.CHANNELS), channelId);
            var resp = await RestService.SendRequestAsync(new StringRestRequest(rb, HttpMethod.Get))
                .ConfigureAwait(false);
            var responseContent = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
            var channelType = (ChannelType) JObject.Parse(responseContent).Value<int>("type");
            switch (channelType)
            {
                case ChannelType.GuildText:
                    return JsonConvert.DeserializeObject<RestTextChannel>(responseContent);
                case ChannelType.GuildVoice:
                    return JsonConvert.DeserializeObject<RestVoiceChannel>(responseContent);
                case ChannelType.GuildCategory:
                    return JsonConvert.DeserializeObject<RestCategory>(responseContent);
                default:
                    return default;
            }
        }

        public async Task<RestGuildChannel> EditChannelAsync(ulong channelId, GuildChannelProperties props)
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.CHANNELS, true), channelId);

            var resp = await RestService
                .SendRequestAsync(new StringRestRequest(rb, HttpMethod.Put, channelId, props.Serialize()))
                .ConfigureAwait(false);
            var responseContent = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
            var channelType = (ChannelType) JObject.Parse(responseContent).Value<int>("type");
            switch (channelType)
            {
                case ChannelType.GuildText:
                    return JsonConvert.DeserializeObject<RestTextChannel>(responseContent);
                case ChannelType.GuildVoice:
                    return JsonConvert.DeserializeObject<RestVoiceChannel>(responseContent);
                case ChannelType.GuildCategory:
                    return JsonConvert.DeserializeObject<RestCategory>(responseContent);
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

        public async Task<RestMessage> CreateMessageAsync(ulong channelId, string content, bool isTTS = false,
            EmbedBuilder embedBuilder = null, params (string fileName, Stream fileStream)[] attachments)
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.CHANNELS, true), channelId)
                .WithPart(RouteSection.Create(Consts.MESSAGES));

            var multipartFields = new Dictionary<string, string>();
            var multipartFiles = new Dictionary<string, (string, Stream)>();

            if (isTTS)
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

            var resp = await RestService.SendRequestAsync(new MultipartRestRequest(rb, HttpMethod.Post, multipartFields,
                multipartFiles, channelId)).ConfigureAwait(false);
            var responseContent = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);

            var message = responseContent.Deserialize<RestMessage>();

            return message;
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

                messages.AddRange(responseContent.Deserialize<RestMessage[]>());

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
            var responseContent = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);

            var message = responseContent.Deserialize<RestMessage>();

            return message;
        }

        public async Task<RestMessage> EditMessageAsync(ulong messageId, ulong channelId, MessageEditProperties props)
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.CHANNELS, true), channelId)
                .WithPart(RouteSection.Create(Consts.MESSAGES), messageId);

            var responseMessage = await RestService.SendRequestAsync(new StringRestRequest(rb, new HttpMethod("PATCH"),
                channelId, props.Serialize()));
            var responseContent = await responseMessage.Content.ReadAsStringAsync();

            return responseContent.Deserialize<RestMessage>();
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
                channelId, new {messages = messageIds}.Serialize()));
        }

        public async Task CreateReactionAsync(ulong messageId, ulong channelId, Emoji emoji)
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.CHANNELS, true), channelId)
                .WithPart(RouteSection.Create(Consts.MESSAGES), messageId)
                .WithPart(RouteSection.Create(Consts.REACTIONS), emoji.ToString())
                .WithPart(RouteSection.Create(Consts.BLANK), Consts.ME);

            _ = await RestService.SendRequestAsync(new StringRestRequest(rb, HttpMethod.Put, channelId))
                .ConfigureAwait(false);
        }

        public async Task DeleteReactionAsync(ulong messageId, ulong channelId, Emoji emoji, ulong? userId = null)
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.CHANNELS, true), channelId)
                .WithPart(RouteSection.Create(Consts.MESSAGES), messageId)
                .WithPart(RouteSection.Create(Consts.REACTIONS), emoji.ToString())
                .WithPart(RouteSection.Create(Consts.BLANK), userId == null ? userId.ToString() : Consts.ME);

            _ = await RestService.SendRequestAsync(new StringRestRequest(rb, HttpMethod.Delete, channelId))
                .ConfigureAwait(false);
        }

        public async Task ClearReactionsAsync(ulong messageId, ulong channelId)
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.CHANNELS, true), channelId)
                .WithPart(RouteSection.Create(Consts.MESSAGES), messageId)
                .WithPart(RouteSection.Create(Consts.REACTIONS), string.Empty);

            _ = await RestService.SendRequestAsync(new StringRestRequest(rb, HttpMethod.Delete, channelId))
                .ConfigureAwait(false);
        }

        public async Task<IReadOnlyCollection<User>> GetReactionsAsync(ulong messageId, ulong channelId, Emoji emoji,
            int limit = 25)
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.CHANNELS, true), channelId)
                .WithPart(RouteSection.Create(Consts.MESSAGES), messageId)
                .WithPart(RouteSection.Create(Consts.REACTIONS), emoji.ToString())
                .WithParameterBuilder(new RouteParameterBuilder()
                    .Add("limit", limit.ToString()));

            var responseMessage = await RestService
                .SendRequestAsync(new StringRestRequest(rb, HttpMethod.Get, channelId)).ConfigureAwait(false);
            var responseContent = await responseMessage.Content.ReadAsStringAsync();

            return responseContent.Deserialize<ReadOnlyCollection<User>>();
        }

        public async Task<GatewayBotResponse> GetSocketUrlAsync()
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.GATEWAY))
                .WithPart(RouteSection.Create(Consts.BOT));

            var resp = await RestService.SendRequestAsync(new StringRestRequest(rb, HttpMethod.Get))
                .ConfigureAwait(false);

            return JsonConvert.DeserializeObject<GatewayBotResponse>(await resp.Content.ReadAsStringAsync()
                .ConfigureAwait(false));
        }

        internal void InvokeLog(LogMessage msg)
            => Log?.Invoke(msg);

        /// <summary>
        ///     Raised when the client receives a log event.
        /// </summary>
        public event Func<LogMessage, Task> Log;
    }
}