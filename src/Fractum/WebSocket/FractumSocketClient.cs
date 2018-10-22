using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.Entities.Extensions;
using Fractum.Entities.WebSocket;
using Fractum.Rest;
using Fractum.Utilities;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;
using Fractum.WebSocket.Hooks;

namespace Fractum.WebSocket
{
    public sealed class FractumSocketClient
    {
        private IPipeline<IPayload<EventModelBase>> _pipeline;

        internal FractumCache Cache;

        internal DateTimeOffset LastSentHeartbeat;

        internal FractumRestClient RestClient;

        internal ISession Session;

        internal SocketWrapper Socket;

        public FractumSocketClient(FractumConfig config)
        {
            Cache = new FractumCache(this);
            Session = new Session();
            PrivateChannels = new ConcurrentDictionary<ulong, CachedDMChannel>();

            RestClient = new FractumRestClient(config);

            RestClient.Log += msg =>
            {
                InvokeLog(msg);
                return Task.CompletedTask;
            };
        }

        internal ConcurrentDictionary<ulong, CachedDMChannel> PrivateChannels { get; }

        public int Latency { get; internal set; }

        /// <summary>
        ///     Use the default <see cref="PayloadPipeline" /> to process gateway messages.
        /// </summary>
        public void UseDefaultPipeline()
        {
            _pipeline = new PayloadPipeline()
                .AddStage(BuildDefaultConnectionStage())
                .AddStage(BuildDefaultEventStage());
        }

        /// <summary>
        ///     Build the default <see cref="IPipelineStage{TData}" /> responsible for handling gateway dispatches.
        /// </summary>
        /// <returns></returns>
        public IPipelineStage<IPayload<EventModelBase>> BuildDefaultEventStage()
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
                .RegisterHook("GUILD_MEMBER_UPDATE", new GuildMemberUpdateHook())
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
        ///     Build the default <see cref="IPipelineStage{TData}" /> responsible for handling the gateway connection.
        /// </summary>
        /// <returns></returns>
        public IPipelineStage<IPayload<EventModelBase>> BuildDefaultConnectionStage()
            => new ConnectionStage(this);

        /// <summary>
        ///     Use a custom <see cref="IPipeline{TData}" /> to process gateway messages.
        /// </summary>
        /// <param name="pipeline"></param>
        public void UsePipeline(IPipeline<IPayload<EventModelBase>> pipeline)
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
                var eventPayload = payload;

                var logMessage = await _pipeline.CompleteAsync(eventPayload).ConfigureAwait(false);
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
                d = new PresenceModel
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

        /// <summary>
        ///     Get or download a <see cref="CachedGuildChannel" /> from/to cache.
        /// </summary>
        /// <param name="channelId">The id of the desired channel.</param>
        /// <returns></returns>
        public CachedEntity<IGuildChannel> GetChannel(ulong channelId)
        {
            var existingChannel = Cache.Guilds
                .Select(g => g.Guild)
                .SelectMany(gc => gc.Channels)
                .FirstOrDefault(cc => cc.Id == channelId);

            Task<IGuildChannel> getFunc()
            {
                return RestClient.GetChannelAsync(channelId);
            }

            return new CachedEntity<IGuildChannel>(existingChannel, getFunc);
        }

        /// <summary>
        ///     Get or download a <see cref="CachedMessage" /> from/to cache.
        /// </summary>
        /// <param name="msgChannel">The channel to download the message from if it doesn't exist in cache.</param>
        /// <param name="messageId">The id of the desired message.</param>
        /// <returns></returns>
        public CachedEntity<IMessage> GetMessage(IMessageChannel msgChannel, ulong messageId)
        {
            var channel = GetChannel(msgChannel.Id);
            var message = (channel.GetValue() as CachedTextChannel)?.Messages.FirstOrDefault(m => m.Id == messageId);

            Task<IMessage> getFunc()
            {
                return RestClient.GetMessageAsync(msgChannel, messageId);
            }

            return new CachedEntity<IMessage>(message, getFunc);
        }

        /// <summary>
        ///     Get a <see cref="CachedGuild" /> from cache.
        /// </summary>
        /// <param name="guildId">The id of the desired guild.</param>
        /// <returns></returns>
        public CachedGuild GetGuild(ulong guildId)
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

        internal void InvokeMessagePinned(CachedTextChannel channel)
            => MessagePinned?.Invoke(channel);

        internal void InvokeGuildCreated(CachedGuild guild)
            => GuildCreated?.Invoke(guild);

        internal void InvokeGuildUnavailable(CachedGuild guild)
            => GuildUnavailable?.Invoke(guild);

        internal void InvokeGuildUpdated(CachedGuild guild)
            => GuildUpdated?.Invoke(guild);

        internal void InvokeMessageCreated(CachedMessage message)
            => MessageCreated?.Invoke(message);

        internal void InvokeMessageUpdated(CachedEntity<IMessage> oldMessage, CachedMessage newMessage)
            => MessageUpdated?.Invoke(oldMessage, newMessage);

        internal void InvokeMessageDeleted(CachedEntity<CachedMessage> cachedMessage)
            => MessageDeleted?.Invoke(cachedMessage);

        internal void InvokeChannelCreated(CachedGuildChannel channel)
            => ChannelCreated?.Invoke(channel);

        internal void InvokeChannelUpdated(CachedEntity<CachedGuildChannel> oldChannel, CachedGuildChannel channel)
            => ChannelUpdated?.Invoke(oldChannel, channel);

        internal void InvokeChannelDeleted(CachedEntity<CachedGuildChannel> channel)
            => ChannelDeleted?.Invoke(channel);

        internal void InvokeMemberBanned(CachedMember member)
            => MemberBanned?.Invoke(member);

        internal void InvokeMemberUnbanned(User user)
            => MemberUnbanned?.Invoke(user);

        internal void InvokeEmojisUpdated(CachedGuild guild)
            => EmojisUpdated?.Invoke(guild);

        internal void InvokeIntegrationsUpdated(CachedGuild guild)
            => IntegrationsUpdated?.Invoke(guild);

        internal void InvokeMemberJoined(CachedMember member)
            => MemberJoined?.Invoke(member);

        internal void InvokeMemberLeft(IUser user)
            => MemberLeft?.Invoke(user);

        internal void InvokeRoleCreated(CachedGuild guild, Role role)
            => RoleCreated?.Invoke(guild, role);

        internal void InvokeRoleUpdated(CachedGuild guild, Role role)
            => RoleUpdated?.Invoke(guild, role);

        internal void InvokeRoleDeleted(CachedGuild guild, Role role)
            => RoleDeleted?.Invoke(guild, role);

        internal void InvokeReactionAdded(CachedReaction reaction)
            => ReactionAdded?.Invoke(reaction);

        internal void InvokeReactionRemoved(CachedReaction reaction)
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
        public event Func<CachedTextChannel, Task> MessagePinned;

        /// <summary>
        ///     Raised when a guild becomes available to the client.
        /// </summary>
        public event Func<CachedGuild, Task> GuildCreated;

        /// <summary>
        ///     Raised when a guild becomes unavailable to the client.
        /// </summary>
        public event Func<CachedGuild, Task> GuildUnavailable;

        /// <summary>
        ///     Raised when a guild is updated.
        /// </summary>
        public event Func<CachedGuild, Task> GuildUpdated;

        /// <summary>
        ///     Raised when a guild is deleted.
        /// </summary>
        public event Func<CachedEntity<CachedGuild>, Task> GuildDeleted;

        /// <summary>
        ///     Raised when a message is created.
        /// </summary>
        public event Func<CachedMessage, Task> MessageCreated;

        /// <summary>
        ///     Raised when a message is edited.
        /// </summary>
        public event Func<CachedEntity<IMessage>, CachedMessage, Task> MessageUpdated;

        /// <summary>
        ///     Raised when a message is deleted.
        /// </summary>
        public event Func<CachedEntity<CachedMessage>, Task> MessageDeleted;

        /// <summary>
        ///     Raised when a channel is created.
        /// </summary>
        public event Func<CachedGuildChannel, Task> ChannelCreated;

        /// <summary>
        ///     Raised when a channel is updated.
        /// </summary>
        public event Func<CachedEntity<CachedGuildChannel>, CachedGuildChannel, Task> ChannelUpdated;

        /// <summary>
        ///     Raised when a channel is deleted.
        /// </summary>
        public event Func<CachedEntity<CachedGuildChannel>, Task> ChannelDeleted;

        /// <summary>
        ///     Raised when a member is banned.
        /// </summary>
        public event Func<CachedMember, Task> MemberBanned;

        /// <summary>
        ///     Raised when a member is unbanned.
        /// </summary>
        public event Func<User, Task> MemberUnbanned;

        /// <summary>
        ///     Raised when emojis in a guild are updated.
        /// </summary>
        public event Func<CachedGuild, Task> EmojisUpdated;

        /// <summary>
        ///     Raised when a guild's integrations are updated.
        /// </summary>
        public event Func<CachedGuild, Task> IntegrationsUpdated;

        /// <summary>
        ///     Raised when a member joins a guild.
        /// </summary>
        public event Func<CachedMember, Task> MemberJoined;

        /// <summary>
        ///     Raised when a member leaves a guild.
        /// </summary>
        public event Func<IUser, Task> MemberLeft;

        /// <summary>
        ///     Raised when a role is created.
        /// </summary>
        public event Func<CachedGuild, Role, Task> RoleCreated;

        /// <summary>
        ///     Raised when a role is updated.
        /// </summary>
        public event Func<CachedGuild, Role, Task> RoleUpdated;

        /// <summary>
        ///     Raised when a role is deleted.
        /// </summary>
        public event Func<CachedGuild, Role, Task> RoleDeleted;

        /// <summary>
        ///     Raised when a reaction is added to a message.
        /// </summary>
        public event Func<CachedReaction, Task> ReactionAdded;

        /// <summary>
        ///     Raised when a reaction is removed from a message.
        /// </summary>
        public event Func<CachedReaction, Task> ReactionRemoved;

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