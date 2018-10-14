using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.Entities.Extensions;
using Fractum.Entities.Properties;
using Fractum.Entities.WebSocket;
using Fractum.Rest;
using Fractum.Utilities;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.Hooks;

namespace Fractum.WebSocket
{
    public sealed class FractumSocketClient
    {
        private IPipeline<Payload> _pipeline;

        internal SocketWrapper Socket;

        internal ISession Session;

        internal FractumCache Cache;

        internal FractumRestClient RestClient;

        internal ConcurrentDictionary<ulong, PrivateChannel> PrivateChannels { get; private set; }

        public FractumSocketClient(FractumConfig config)
        {
            Cache = new FractumCache(this);
            Session = new Session();
            PrivateChannels = new ConcurrentDictionary<ulong, PrivateChannel>();

            RestClient = new FractumRestClient(config);

            RestClient.Log += Log;
        }

        /// <summary>
        ///     Use the default <see cref="PayloadPipeline"/> to process gateway messages.
        /// </summary>
        public void UseDefaultPipeline()
        {
            _pipeline = new PayloadPipeline()
                .AddStage(BuildDefaultConnectionStage())
                .AddStage(BuildDefaultEventStage());
        }

        /// <summary>
        ///     Build the default <see cref="IPipelineStage{TData}"/> responsible for handling gateway dispatches.
        /// </summary>
        /// <returns></returns>
        public IPipelineStage<Payload> BuildDefaultEventStage()
            => new EventStage(this)
                .RegisterHook("READY", new ReadyHook())
                .RegisterHook("PRESENCE_UPDATE", new PresenceUpdateHook())
                .RegisterHook("GUILD_CREATE", new GuildCreateHook())
                .RegisterHook("GUILD_UPDATE", new GuildUpdateHook())
                .RegisterHook("GUILD_MEMBER_UPDATE", new PresenceUpdateHook())
                .RegisterHook("GUILD_MEMBERS_CHUNK", new GuildMembersChunkHook())
                .RegisterHook("CHANNEL_CREATE", new ChannelCreateHook())
                .RegisterHook("CHANNEL_UPDATE", new ChannelUpdateHook())
                .RegisterHook("CHANNEL_DELETE", new ChannelDeleteHook())
                .RegisterHook("CHANNEL_PINS_UPDATE", new ChannelPinsUpdateHook())
                .RegisterHook("MESSAGE_CREATE", new MessageReceivedHook());

        /// <summary>
        ///     Build the default <see cref="IPipelineStage{TData}"/> responsible for handling the gateway connection.
        /// </summary>
        /// <returns></returns>
        public IPipelineStage<Payload> BuildDefaultConnectionStage()
            => new ConnectionStage(this);

        /// <summary>
        ///     Use a custom <see cref="IPipeline{TData}"/> to process gateway messages.
        /// </summary>
        /// <param name="pipeline"></param>
        public void UsePipeline(IPipeline<Payload> pipeline)
            => _pipeline = pipeline;

        /// <summary>
        ///     Retrieve the gateway url and prepare the client for connection.
        /// </summary>
        /// <returns></returns>
        public async Task InitialiseAsync()
        {
            var gatewayInfo = await RestClient.GetSocketUrlAsync().ConfigureAwait(false);
            if (gatewayInfo.SessionStartLimit["remaining"] <= 0)
                throw new InvalidOperationException("No new sessions can be started.");

            Socket = new SocketWrapper(new Uri(gatewayInfo.Url + Consts.GATEWAY_PARAMS));

            if (_pipeline is null)
                UseDefaultPipeline();

            Socket.PayloadReceived += async payload =>
            {
                var logMessage = await _pipeline.CompleteAsync(payload).ConfigureAwait(false);
                if (logMessage != null)
                    InvokeLog(logMessage);
            };
        }

        /// <summary>
        ///     Update the presence of your bot application.
        /// </summary>
        /// <param name="name">The name of the activity.</param>
        /// <param name="type">The type of the activity.</param>
        /// <param name="status">The status of your bot.</param>
        /// <returns></returns>
        public Task UpdatePresenceAsync(string name, ActivityType type = ActivityType.Playing,
            Status status = Status.Online)
        {
            var payload = new
            {
                op = OpCode.StatusUpdate,
                d = new Presence
                {
                    Activity = new Activity
                    {
                        Name = name,
                        Type = type
                    },
                    Status = status
                }
            }.Serialize();
            return Socket.SendMessageAsync(payload);
        }

        /// <summary>
        ///     Request the members of a guild to be downloaded asynchronously from the gateway and pulled into cache.
        /// </summary>
        /// <param name="guildId">The guild to download members for.</param>
        /// <param name="queryString">A query string to filter members by.</param>
        /// <param name="limit">The amount of members to be downloaded.</param>
        /// <returns></returns>
        public Task RequestMembersAsync(ulong guildId, string queryString = null, int limit = 0)
        {
            var memberRequest = new SendPayload
            {
                op = OpCode.RequestGuildMembers,
                d = new
                {
                    guild_id = guildId,
                    query = queryString ?? string.Empty,
                    limit
                }
            }.Serialize();

            return Socket.SendMessageAsync(memberRequest);
        }

        internal async Task<Message> CreateMessageAsync(IMessageChannel channel, string content, bool isTTS = false,
            EmbedBuilder embedBuilder = null, params (string fileName, Stream fileStream)[] attachments)
        {
            var message = await RestClient.CreateMessageAsync(channel, content, isTTS, embedBuilder, attachments).ConfigureAwait(false);
            Cache.PopulateMessage(message);

            return message;
        }

        internal async Task<Message> EditMessageAsync(Message message, MessageEditProperties props)
        {
            var newMessage = await RestClient.EditMessageAsync(message, props);
            Cache.PopulateMessage(newMessage);

            return newMessage;
        }

        internal async Task<GuildChannel> EditChannelAsync(ulong channelId, GuildChannelProperties props)
        {
            var channel = await RestClient.EditChannelAsync(channelId, props);
            Cache.PopulateChannel(channel);

            return channel;
        }

        private async Task<Message> GetMessageAsync(IMessageChannel channel, ulong messageId)
        {
            var message = await RestClient.GetMessageAsync(channel, messageId).ConfigureAwait(false);
            Cache.PopulateMessage(message);

            return message;
        }

        internal async Task<IReadOnlyCollection<Message>> GetMessagesAsync(IMessageChannel channel, ulong messageId, int limit)
        {
            var messages = await RestClient.GetMessagesAsync(channel, messageId, limit).ConfigureAwait(false);
            foreach (var message in messages)
                Cache.PopulateMessage(message);

            return messages;
        }

        /// <summary>
        ///     Get or download a <see cref="GuildChannel"/> from/to cache.
        /// </summary>
        /// <param name="channelId">The id of the desired channel.</param>
        /// <returns></returns>
        public CachedEntity<GuildChannel> GetChannel(ulong channelId)
        {
            var existingChannel = Cache.Guilds
                .Select(g => g.Value)
                .SelectMany(gc => gc.Channels)
                .FirstOrDefault(cc => cc.Id == channelId);
            
            return new CachedEntity<GuildChannel>(existingChannel, this.RestClient.GetChannelAsync(channelId));
        }

        /// <summary>
        ///     Get or download a <see cref="Message"/> from/to cache.
        /// </summary>
        /// <param name="msgChannel">The channel to download the message from if it doesn't exist in cache.</param>
        /// <param name="messageId">The id of the desired message.</param>
        /// <returns></returns>
        public CachedEntity<Message> GetMessage(IMessageChannel msgChannel, ulong messageId)
        {
            var channel = GetChannel(msgChannel.Id);
            var message = (channel.GetValue() as TextChannel)?.Messages.FirstOrDefault(m => m.Id == messageId);

            return new CachedEntity<Message>(message, this.GetMessageAsync(msgChannel, messageId));
        }

        /// <summary>
        ///     Get a <see cref="Guild"/> from cache.
        /// </summary>
        /// <param name="guildId">The id of the desired guild.</param>
        /// <returns></returns>
        public Guild GetGuild(ulong guildId)
            => Cache.GetGuild(guildId);

        /// <summary>
        ///     Connect and listen on the socket. This method does not block.
        /// </summary>
        /// <returns></returns>
        public Task ConnectAsync()
            => Socket.ConnectAsync();

        /// <summary>
        ///     Disconnect from the socket.
        /// </summary>
        /// <returns></returns>
        public Task DisconnectAsync()
            => Socket.DisconnectAsync(WebSocketCloseStatus.NormalClosure);

        internal void InvokeLog(LogMessage message)
            => Log?.Invoke(message);

        internal void InvokeMessagePinned(TextChannel channel)
            => MessagePinned?.Invoke(channel);

        internal void InvokeGuildCreated(Guild guild)
            => GuildCreated?.Invoke(guild);

        internal void InvokeGuildUnavailable(Guild guild)
            => GuildUnavailable?.Invoke(guild);

        internal void InvokeGuildUpdated(Guild guild)
            => GuildUpdated?.Invoke(guild);

        internal void InvokeMessageCreated(Message message)
            => MessageCreated?.Invoke(message);

        internal void InvokeMessageUpdated(CachedEntity<Message> oldMessage, Message newMessage)
            => MessageUpdated?.Invoke(oldMessage, newMessage);

        internal void InvokeChannelCreated(GuildChannel channel)
            => ChannelCreated?.Invoke(channel);

        internal void InvokeChannelUpdated(CachedEntity<GuildChannel> oldChannel, GuildChannel channel)
            => ChannelUpdated?.Invoke(oldChannel, channel);

        internal void InvokeChannelDeleted(CachedEntity<GuildChannel> channel)
            => ChannelDeleted?.Invoke(channel);

        internal void InvokeReady()
            => Ready?.Invoke();

        /// <summary>
        ///     Raised when a message is pinned in a text channel.
        /// </summary>
        public event Func<TextChannel, Task> MessagePinned;

        /// <summary>
        ///     Raised when a guild becomes available to the client.
        /// </summary>
        public event Func<Guild, Task> GuildCreated;

        /// <summary>
        ///     Raised when a guild becomes unavailable to the client.
        /// </summary>
        public event Func<Guild, Task> GuildUnavailable;

        /// <summary>
        ///     Raised when a guild is updated.
        /// </summary>
        public event Func<Guild, Task> GuildUpdated;

        /// <summary>
        ///     Raised when a guild is deleted.
        /// </summary>
        public event Func<CachedEntity<Guild>, Task> GuildDeleted;

        /// <summary>
        ///     Raised when a message is created.
        /// </summary>
        public event Func<Message, Task> MessageCreated;

        /// <summary>
        ///     Raised when a message is edited.
        /// </summary>
        public event Func<CachedEntity<Message>, Message, Task> MessageUpdated;

        /// <summary>
        ///     Raised when a message is deleted.
        /// </summary>
        public event Func<CachedEntity<Message>> MessageDeleted;

        /// <summary>
        ///     Raised when a channel is created.
        /// </summary>
        public event Func<GuildChannel, Task> ChannelCreated;

        /// <summary>
        ///     Raised when a channel is updated.
        /// </summary>
        public event Func<CachedEntity<GuildChannel>, GuildChannel, Task> ChannelUpdated;

        /// <summary>
        ///     Raised when a channel is deleted.
        /// </summary>
        public event Func<CachedEntity<GuildChannel>, Task> ChannelDeleted;

        /// <summary>
        ///     Raised when the client has successfully identified and is ready to receive and send events and commands.
        /// </summary>
        public event Func<Task> Ready;

        /// <summary>
        ///     Raised by the client when log-worthy events are encountered.
        /// </summary>
        public event Func<LogMessage, Task> Log;
    }
}