using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.Entities.Extensions;
using Fractum.Entities.Properties;
using Fractum.Rest.Utils;
using Fractum.WebSocket;
using Fractum.WebSocket.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fractum.Rest
{
    public sealed class FractumRestClient
    {
        private FractumRestService _rest;

        internal FractumConfig Config;

        public FractumRestClient(FractumConfig config)
        {
            Config = config;

            _rest = new FractumRestService(config);

            _rest.Log += Log;
        }

        public Task TriggerTypingAsync(ulong channelId)
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.CHANNELS, true), channelId)
                .WithPart(RouteSection.Create(Consts.TYPING));
            return _rest.SendRequestAsync(new StringRestRequest(rb, HttpMethod.Post, channelId));
        }

        public async Task<GuildChannel> GetChannelAsync(ulong channelId)
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.CHANNELS), channelId);
            var resp = await _rest.SendRequestAsync(new StringRestRequest(rb, HttpMethod.Get)).ConfigureAwait(false);
            var responseContent = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
            var channelType = (ChannelType)JObject.Parse(responseContent).Value<int>("type");
            switch (channelType)
            {
                case ChannelType.GuildText:
                    return JsonConvert.DeserializeObject<TextChannel>(responseContent);
                case ChannelType.GuildVoice:
                    return JsonConvert.DeserializeObject<VoiceChannel>(responseContent);
                case ChannelType.GuildCategory:
                    return JsonConvert.DeserializeObject<Category>(responseContent);
                default:
                    return default;
            }
        }

        public async Task<GuildChannel> EditChannelAsync(ulong channelId, GuildChannelProperties props)
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.CHANNELS, true), channelId);

            var resp = await _rest.SendRequestAsync(new StringRestRequest(rb, HttpMethod.Put, channelId, props.Serialize())).ConfigureAwait(false);
            var responseContent = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
            var channelType = (ChannelType)JObject.Parse(responseContent).Value<int>("type");
            switch (channelType)
            {
                case ChannelType.GuildText:
                    return JsonConvert.DeserializeObject<TextChannel>(responseContent);
                case ChannelType.GuildVoice:
                    return JsonConvert.DeserializeObject<VoiceChannel>(responseContent);
                case ChannelType.GuildCategory:
                    return JsonConvert.DeserializeObject<Category>(responseContent);
                default:
                    return default;
            }
        }

        public async Task DeleteChannelAsync(ulong channelId)
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.CHANNELS, true), channelId);

            _ = await _rest.SendRequestAsync(new StringRestRequest(rb, HttpMethod.Delete, channelId)).ConfigureAwait(false);
        }

        public async Task<Message> CreateMessageAsync(IMessageChannel channel, string content, bool isTTS = false,
            EmbedBuilder embedBuilder = null, params (string fileName, Stream fileStream)[] attachments)
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.CHANNELS, true), channel.Id)
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

            var resp = await _rest.SendRequestAsync(new MultipartRestRequest(rb, HttpMethod.Post, multipartFields,
                multipartFiles, channel.Id)).ConfigureAwait(false);
            var responseContent = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);

            var message = responseContent.Deserialize<Message>();


            return message;
        }

        public async Task<IReadOnlyCollection<Message>> GetMessagesAsync(IMessageChannel channel, ulong messageId, int target)
        {
            var messages = new List<Message>();
            do
            {
                var limit = target < 100 ? target : 100;
                target -= limit;

                var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.CHANNELS, true), channel.Id)
                .WithPart(RouteSection.Create(Consts.MESSAGES))
                    .WithParameterBuilder(new RouteParameterBuilder()
                        .Add("before", messageId.ToString())
                        .Add("limit", limit.ToString()));

                var resp = await _rest.SendRequestAsync(new StringRestRequest(rb, HttpMethod.Get, channel.Id)).ConfigureAwait(false);
                var responseContent = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);

                messages.AddRange(responseContent.Deserialize<Message[]>());

                messageId = messages[messages.Count - 1].Id;

            } while (target > 0);

            return messages;
        }

        public async Task<Message> GetMessageAsync(IMessageChannel channel, ulong messageId)
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.CHANNELS, true), channel.Id)
                .WithPart(RouteSection.Create(Consts.MESSAGES), messageId);

            var resp = await _rest.SendRequestAsync(new StringRestRequest(rb, HttpMethod.Get, channel.Id)).ConfigureAwait(false);
            var responseContent = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);

            var message = responseContent.Deserialize<Message>();

            return message;
        }

        public async Task<Message> EditMessageAsync(Message message, MessageEditProperties props)
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.CHANNELS, true), message.Channel.Id)
                .WithPart(RouteSection.Create(Consts.MESSAGES), message.Id);

            var responseMessage = await _rest.SendRequestAsync(new StringRestRequest(rb, new HttpMethod("PATCH"),
                message.Channel.Id, props.Serialize()));
            var responseContent = await responseMessage.Content.ReadAsStringAsync();

            return responseContent.Deserialize<Message>();
        }

        public Task DeleteMessageAsync(Message message)
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.CHANNELS, true), message.Channel.Id)
                .WithPart(RouteSection.Create(Consts.MESSAGES), message.Id);

            return _rest.SendRequestAsync(new StringRestRequest(rb, HttpMethod.Delete,
                message.Channel.Id));
        }

        public Task DeleteMessagesAsync(ulong channelId, IEnumerable<ulong> messageIds)
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.CHANNELS, true), channelId)
                .WithPart(RouteSection.Create(Consts.MESSAGES))
                .WithPart(RouteSection.Create(Consts.BULK_DELETE));

            return _rest.SendRequestAsync(new StringRestRequest(rb, HttpMethod.Post,
                channelId, new { messages = messageIds }.Serialize()));
        }

        public async Task CreateReactionAsync(Message message, Emoji emoji)
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.CHANNELS, true), message.Channel.Id)
                .WithPart(RouteSection.Create(Consts.MESSAGES), message.Id)
                .WithPart(RouteSection.Create(Consts.REACTIONS), emoji.ToString())
                .WithPart(RouteSection.Create(Consts.BLANK), Consts.ME);

            _ = await _rest.SendRequestAsync(new StringRestRequest(rb, HttpMethod.Put, message.Channel.Id)).ConfigureAwait(false);
        }

        public async Task DeleteReactionAsync(Message message, Emoji emoji, ulong? userId = null)
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.CHANNELS, true), message.Channel.Id)
                .WithPart(RouteSection.Create(Consts.MESSAGES), message.Id)
                .WithPart(RouteSection.Create(Consts.REACTIONS), emoji.ToString())
                .WithPart(RouteSection.Create(Consts.BLANK), userId == null ? userId.ToString() : Consts.ME);

            _ = await _rest.SendRequestAsync(new StringRestRequest(rb, HttpMethod.Delete, message.Channel.Id)).ConfigureAwait(false);
        }

        public async Task ClearReactionsAsync(Message message)
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.CHANNELS, true), message.Channel.Id)
                .WithPart(RouteSection.Create(Consts.MESSAGES), message.Id)
                .WithPart(RouteSection.Create(Consts.REACTIONS), string.Empty);

            _ = await _rest.SendRequestAsync(new StringRestRequest(rb, HttpMethod.Delete, message.Channel.Id)).ConfigureAwait(false);
        }

        public async Task<IReadOnlyCollection<User>> GetReactionsAsync(Message message, Emoji emoji, int limit = 25)
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.CHANNELS, true), message.Channel.Id)
                .WithPart(RouteSection.Create(Consts.MESSAGES), message.Id)
                .WithPart(RouteSection.Create(Consts.REACTIONS), emoji.ToString())
                .WithParameterBuilder(new RouteParameterBuilder()
                    .Add("limit", limit.ToString()));

            var responseMessage = await _rest.SendRequestAsync(new StringRestRequest(rb, HttpMethod.Get, message.Channel.Id)).ConfigureAwait(false);
            var responseContent = await responseMessage.Content.ReadAsStringAsync();

            return responseContent.Deserialize<ReadOnlyCollection<User>>();
        }

        public async Task<GatewayBotResponse> GetSocketUrlAsync()
        {
            var rb = new RouteBuilder()
                .WithPart(RouteSection.Create(Consts.GATEWAY))
                .WithPart(RouteSection.Create(Consts.BOT));

            var resp = await _rest.SendRequestAsync(new StringRestRequest(rb, HttpMethod.Get)).ConfigureAwait(false);

            return JsonConvert.DeserializeObject<GatewayBotResponse>(await resp.Content.ReadAsStringAsync().ConfigureAwait(false));
        }

        internal void InvokeLog(LogMessage msg)
            => Log?.Invoke(msg);

        /// <summary>
        ///     Raised when the client receives a log event.
        /// </summary>
        public event Func<LogMessage, Task> Log;
    }
}