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

        internal DateTimeOffset LastSentHeartbeat;

        internal ConcurrentDictionary<ulong, PrivateChannel> PrivateChannels { get; private set; }

        public int Latency { get; internal set; }

        public FractumSocketClient(FractumConfig config)
        {
            Cache = new FractumCache(this);
            Session = new Session();
            PrivateChannels = new ConcurrentDictionary<ulong, PrivateChannel>();

            RestClient = new FractumRestClient(config);

            RestClient.Log += (msg) =>
            {
                InvokeLog(msg);
                return Task.CompletedTask;
            };
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
                .RegisterHook("GUILD_DELETE", new GuildDeleteHook())
                .RegisterHook("GUILD_BAN_ADD", new BanAddHook())
                .RegisterHook("GUILD_BAN_REMOVE", new BanRemoveHook())
                .RegisterHook("GUILD_EMOJIS_UPDATE", new EmojisUpdateHook())
                .RegisterHook("GUILD_INTEGRATIONS_UPDATE", new IntegrationsUpdatedHook())
                .RegisterHook("GUILD_MEMBER_ADD", new GuildMemberAddHook())
                .RegisterHook("GUILD_MEMBER_REMOVE", new GuildMemberRemoveHook())
                .RegisterHook("GUILD_MEMBER_UPDATE", new PresenceUpdateHook())
                .RegisterHook("GUILD_MEMBERS_CHUNK", new GuildMembersChunkHook())
                .RegisterHook("GUILD_ROLE_CREATE", new RoleCreateHook())
                .RegisterHook("GUILD_ROLE_UPDATE", new RoleUpdateHook())
                .RegisterHook("GUILD_ROLE_DELETE", new RoleDeleteHook())
                .RegisterHook("CHANNEL_CREATE", new ChannelCreateHook())
                .RegisterHook("CHANNEL_UPDATE", new ChannelUpdateHook())
                .RegisterHook("CHANNEL_DELETE", new ChannelDeleteHook())
                .RegisterHook("CHANNEL_PINS_UPDATE", new ChannelPinsUpdateHook())
                .RegisterHook("MESSAGE_CREATE", new MessageCreateHook())
                .RegisterHook("MESSAGE_UPDATE", new MessageUpdateHook())
                .RegisterHook("MESSAGE_DELETE", new MessageDeleteHook())
                .RegisterHook("MESSAGE_REACTION_ADD", new ReactionAddHook())
                .RegisterHook("MESSAGE_REACTION_REMOVE", new ReactionRemoveHook())
                .RegisterHook("MESSAGE_REACTION_REMOVE_ALL", new ReactionsClearHook())
                .RegisterHook("USER_UPDATE", new UserUpdateHook())
                //.RegisterHook("TYPING_START", new TypingStartHook())
                //.RegisterHook("VOICE_STATE_UPDATE", new VoiceStateUpdateHook())
                //.RegisterHook("WEBHOOKS_UPDATE", new WebhooksUpdateHook());
                ;

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

            message.Guild = Cache[message.GuildId ?? default];

            return message;
        }

        internal async Task<Message> EditMessageAsync(Message message, MessageEditProperties props)
        {
            var newMessage = await RestClient.EditMessageAsync(message, props).ConfigureAwait(false);

            message.Guild = Cache[message.GuildId ?? default];

            return newMessage;
        }

        internal async Task<GuildChannel> GetChannelAsync(ulong channelId)
        {
            var channel = await RestClient.GetChannelAsync(channelId).ConfigureAwait(false);

            Cache.Populate(channel);

            return channel;
        }

        internal async Task<GuildChannel> EditChannelAsync(ulong channelId, GuildChannelProperties props)
        {
            var channel = await RestClient.EditChannelAsync(channelId, props).ConfigureAwait(false);

            Cache.Populate(channel);

            return channel;
        }

        private async Task<Message> GetMessageAsync(IMessageChannel channel, ulong messageId)
        {
            var message = await RestClient.GetMessageAsync(channel, messageId).ConfigureAwait(false);

            message.Guild = Cache[message];
            message.WithClient(message.Guild?.Client);
            return message;
        }

        internal async Task<IReadOnlyCollection<Message>> GetMessagesAsync(IMessageChannel channel, ulong messageId, int limit)
        {
            var messages = await RestClient.GetMessagesAsync(channel, messageId, limit).ConfigureAwait(false);
            var guild = Cache[messages.First()];
            foreach (var message in messages)
                message.Guild = guild;

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
                .Select(g => g.Guild)
                .SelectMany(gc => gc.Channels)
                .FirstOrDefault(cc => cc.Id == channelId);

            Task<GuildChannel> getFunc() => GetChannelAsync(channelId);
            return new CachedEntity<GuildChannel>(existingChannel, getFunc);
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

            Task<Message> getFunc() => GetMessageAsync(msgChannel, messageId);
            return new CachedEntity<Message>(message, getFunc);
        }

        /// <summary>
        ///     Get a <see cref="Guild"/> from cache.
        /// </summary>
        /// <param name="guildId">The id of the desired guild.</param>
        /// <returns></returns>
        public Guild GetGuild(ulong guildId)
            => Cache[guildId]?.Guild;

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

        internal void InvokeMessageDeleted(CachedEntity<Message> cachedMessage)
            => MessageDeleted?.Invoke(cachedMessage);

        internal void InvokeChannelCreated(GuildChannel channel)
            => ChannelCreated?.Invoke(channel);

        internal void InvokeChannelUpdated(CachedEntity<GuildChannel> oldChannel, GuildChannel channel)
            => ChannelUpdated?.Invoke(oldChannel, channel);

        internal void InvokeChannelDeleted(CachedEntity<GuildChannel> channel)
            => ChannelDeleted?.Invoke(channel);

        internal void InvokeMemberBanned(GuildMember member)
            => MemberBanned?.Invoke(member);

        internal void InvokeMemberUnbanned(User user)
            => MemberUnbanned?.Invoke(user);

        internal void InvokeEmojisUpdated(Guild guild)
            => EmojisUpdated?.Invoke(guild);

        internal void InvokeIntegrationsUpdated(Guild guild)
            => IntegrationsUpdated?.Invoke(guild);

        internal void InvokeMemberJoined(GuildMember member)
            => MemberJoined?.Invoke(member);

        internal void InvokeMemberLeft(IUser user)
            => MemberLeft?.Invoke(user);

        internal void InvokeRoleCreated(Guild guild, Role role)
            => RoleCreated?.Invoke(guild, role);

        internal void InvokeRoleUpdated(Guild guild, Role role)
            => RoleUpdated?.Invoke(guild, role);

        internal void InvokeRoleDeleted(Guild guild, Role role)
            => RoleDeleted?.Invoke(guild, role);

        internal void InvokeReactionAdded(Reaction reaction)
            => ReactionAdded?.Invoke(reaction);

        internal void InvokeReactionRemoved(Reaction reaction)
          => ReactionAdded?.Invoke(reaction);

        internal void InvokeReactionsCleared(ulong messageId, ulong channelId, ulong? guildId)
            => ReactionsCleared?.Invoke(messageId, channelId, guildId);

        internal void InvokeUserUpdated(CachedEntity<User> oldUser, User user)
            => UserUpdated?.Invoke(oldUser, user);

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
        public event Func<CachedEntity<Message>, Task> MessageDeleted;

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
        ///     Raised when a member is banned.
        /// </summary>
        public event Func<GuildMember, Task> MemberBanned;

        /// <summary>
        ///     Raised when a member is unbanned.
        /// </summary>
        public event Func<User, Task> MemberUnbanned;

        /// <summary>
        ///     Raised when emojis in a guild are updated.
        /// </summary>
        public event Func<Guild, Task> EmojisUpdated;

        /// <summary>
        ///     Raised when a guild's integrations are updated.
        /// </summary>
        public event Func<Guild, Task> IntegrationsUpdated;

        /// <summary>
        ///     Raised when a member joins a guild.
        /// </summary>
        public event Func<GuildMember, Task> MemberJoined;

        /// <summary>
        ///     Raised when a member leaves a guild.
        /// </summary>
        public event Func<IUser, Task> MemberLeft;

        /// <summary>
        ///     Raised when a role is created.
        /// </summary>
        public event Func<Guild, Role, Task> RoleCreated;

        /// <summary>
        ///     Raised when a role is updated.
        /// </summary>
        public event Func<Guild, Role, Task> RoleUpdated;

        /// <summary>
        ///     Raised when a role is deleted.
        /// </summary>
        public event Func<Guild, Role, Task> RoleDeleted;

        /// <summary>
        ///     Raised when a reaction is added to a message.
        /// </summary>
        public event Func<Reaction, Task> ReactionAdded;

        /// <summary>
        ///     Raised when a reaction is removed from a message.
        /// </summary>
        public event Func<Reaction, Task> ReactionRemoved;

        /// <summary>
        ///     Raised when the reactions for a message are cleared.
        /// </summary>
        public event Func<ulong, ulong, ulong?, Task> ReactionsCleared;

        /// <summary>
        ///     Raised when a user is updated.
        /// </summary>
        public event Func<CachedEntity<User>, User, Task> UserUpdated;

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