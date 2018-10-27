using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.Entities.WebSocket;
using Fractum.Extensions;
using Fractum.Rest;
using Fractum.WebSocket.EventModels;
using Fractum.WebSocket.Hooks;
using Microsoft.Extensions.DependencyInjection;

namespace Fractum.WebSocket
{
    public sealed class FractumSocketClient
    {
        private IPipeline<IPayload<EventModelBase>> _pipeline;

        internal Timer HeartbeatTimer { get; set; }

        internal ISocketCache<ISyncedGuild> Cache;

        internal DateTimeOffset LastSentHeartbeat;

        internal FractumRestClient RestClient;

        internal ISession Session;

        internal SocketWrapper Socket;

        internal ServiceCollection PipelineServices;

        internal ConcurrentDictionary<ulong, CachedDMChannel> PrivateChannels { get; }

        public Func<LogMessage, Task> LogFunc;

        public IKeyedEnumerable<ulong, CachedGuild> Guilds => new KeyedGuildWrapper(Cache);

        public IKeyedEnumerable<ulong, CachedGuildChannel> Channels => new KeyedChannelWrapper(Cache);

        public IKeyedEnumerable<ulong, User> Users => new KeyedUserWrapper(Cache);

        public int Latency { get; internal set; }

        public FractumSocketClient(FractumConfig config)
        {
            Cache = new FractumCache(this);
            Session = new Session();
            PrivateChannels = new ConcurrentDictionary<ulong, CachedDMChannel>();
            RestClient = new FractumRestClient(config);

            PipelineServices = new ServiceCollection();

            RestClient.Log += msg =>
            {
                InvokeLog(msg);
                return Task.CompletedTask;
            };
        }

        /// <summary>
        ///     Use the default <see cref="PayloadPipeline" /> to process gateway messages.
        /// </summary>
        public FractumSocketClient GetPipeline()
        {
            _pipeline = _pipeline ?? new PayloadPipeline();

            return this;
        }

        /// <summary>
        ///     Add the <see cref="EventStage"/> responsible for maintaining cache and handling gateway dispatches to the pipeline.
        /// </summary>
        /// <returns></returns>
        public FractumSocketClient AddEventStage()
        {
            _pipeline?.AddStage(new EventStage(this)
                    .RegisterHook("READY", new ReadyHook())
                    .RegisterHook("RESUMED", new ResumedEventHook())
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
            );

            return this;
        }

        /// <summary>
        ///     Add the <see cref="ConnectionStage"/> responsible for connection logic to the pipeline.
        /// </summary>
        /// <returns></returns>
        public FractumSocketClient AddConnectionStage()
        {
            _pipeline.AddStage(new ConnectionStage());

            return this;
        }

        /// <summary>
        ///     Add an <see cref="IPipelineStage{TData}"/> to the pipeline.
        /// </summary>
        /// <typeparam name="T">Type of the stage to be added</typeparam>
        /// <param name="ctorParameters">Optional parameters to be used when constructing the stage</param>
        /// <returns></returns>
        public FractumSocketClient AddStage<T>(params object[] ctorParameters)
            where T : IPipelineStage<IPayload<EventModelBase>>
        {
            _pipeline.AddStage((IPipelineStage<IPayload<EventModelBase>>) Activator.CreateInstance(typeof(T), ctorParameters));

            return this;
        }

        /// <summary>
        ///     Add an <see cref="IPipelineStage{TData}"/> to the pipeline.
        /// </summary>
        /// <typeparam name="T">Type of the stage to be added</typeparam>
        /// <param name="inst">Instance of the stage to be added to the pipeline</param>
        /// <returns></returns>
        public FractumSocketClient AddStage<T>(T inst)
            where T : IPipelineStage<IPayload<EventModelBase>>
        {
            _pipeline.AddStage(inst);

            return this;
        }

        /// <summary>
        ///     Get the <see cref="ServiceCollection"/> containing services accessible to pipeline stages during execution.
        /// </summary>
        /// <returns></returns>
        public ServiceCollection GetPipelineServices() => PipelineServices;

        #region Discord Commands

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
                return RestClient.GetMessageAsync(msgChannel.Id, messageId);
            }

            return new CachedEntity<IMessage>(message, getFunc);
        }

        /// <summary>
        ///     Get a <see cref="CachedGuild" /> from cache.
        /// </summary>
        /// <param name="guildId">The id of the desired guild.</param>
        /// <returns></returns>
        public CachedGuild GetGuild(ulong guildId)
            => Cache.TryGetGuild(guildId, out var guild) ? guild.Guild : default;

        #endregion

        /// <summary>
        ///     Retrieve connection details from the gateway and prepare the client for connection.
        /// </summary>
        /// <returns></returns>
        public async Task GetConnectionInfoAsync()
        {
            var gatewayInfo = await RestClient.GetSocketUrlAsync().ConfigureAwait(false);
            if (gatewayInfo.SessionStartLimit["remaining"] <= 0)
                throw new InvalidOperationException("No new sessions can be started.");

            Socket = new SocketWrapper(new Uri(gatewayInfo.Url + Consts.GATEWAY_PARAMS));

            Socket.ConnectionClosed += ReconnectAsync;

            Socket.PayloadReceived += async payload =>
            {
                var eventPayload = payload;

                var logMessage = await _pipeline.CompleteAsync(eventPayload, new PipelineContext(this)).ConfigureAwait(false);
                if (logMessage != null)
                    InvokeLog(logMessage);
            };
        }

        /// <summary>
        ///     Connect and listen on the socket. This method does not block.
        /// </summary>
        /// <returns></returns>
        public Task StartAsync()
            =>  Socket.ConnectAsync();

        /// <summary>
        ///     Disconnect from the socket.
        /// </summary>
        /// <returns></returns>
        public Task StopAsync()
            => Socket.DisconnectAsync(WebSocketCloseStatus.NormalClosure);

        #region Socket Logic

        /// <summary>
        ///     Handle socket closures and subsequent reconnections.
        /// </summary>
        /// <param name="status">Details of the closure.</param>
        /// <returns></returns>
        internal async Task ReconnectAsync(WebSocketCloseStatus status)
        {
            Session.WaitingForACK = false; // We've been disconnected, reset checks for zombied connections.
            Session.ReconnectionAttempts = 0;

            HeartbeatTimer.Dispose();

            var statusCode = (int)status;

            InvokeLog(new LogMessage(nameof(ConnectionStage),
                $"Disconnected from the gateway with error {statusCode}:{GatewayCloseCode.GetCodeName(statusCode)}.",
                LogSeverity.Error));

            if (statusCode == 1000 || statusCode == 4001 || statusCode == 4006)
                Session.Invalidated = true; // When the connection is re-established don't try to resume, re-identify.

            if (Session.Reconnecting)
                return; // We are trying to resume already and these disconnections are just failed reconnects.
            var backoffPower = 1;
            var backoff = 2;

            InvokeLog(new LogMessage(nameof(ConnectionStage), "Attempting to reconnect...",
                LogSeverity.Warning));

            Session.Reconnecting = true; // Lock other closed event handlers and op2 Handling
            do
            {
                // Make 3 attempts to connect (and resume) then try to refetch the gateway url. After the first 3 attempts we will only attempt to identify.
                //if (Session.ReconnectionAttempts != 0 && Session.ReconnectionAttempts % 3 == 0)
                //    try
                //    {
                //        // Fetch connection info from the gateway with new Url: 
                //        var gatewayInfo = await RestClient.GetSocketUrlAsync()
                //            .ContinueWith(task => !task.IsFaulted ? task.Result : default);
                //        if (gatewayInfo?.SessionStartLimit["remaining"] == 0)
                //            throw new GatewayException("No new sessions can be started");
                //        if (gatewayInfo != null)
                //            Session.GatewayUrl = gatewayInfo.Url;
                //        Socket.UpdateUrl(new Uri(Session.GatewayUrl + Consts.GATEWAY_PARAMS));
                //    }
                //    // If there's no network connection this will fail, don't update with new session data.
                //    catch (Exception)
                //    {
                //        InvokeLog(new LogMessage(nameof(ConnectionStage),
                //            "Failed to update session with new gateway url", LogSeverity.Warning));
                //    }

                var computedBackoff =
                    (int)Math.Pow(backoff, backoffPower) *
                    1000; // Keep raising our backoff up to a maximum of 900 seconds.

                await Task.Delay(computedBackoff <= 900000 ? computedBackoff : 900000);

                InvokeLog(new LogMessage(nameof(ConnectionStage),
                    $"Reconnection attempt {++Session.ReconnectionAttempts}",
                    LogSeverity.Warning));

                await Socket.ConnectAsync(); // Try to reconnect

                backoffPower++;
            } while (Socket.State != WebSocketState.Open); // Socket isn't open yet
            
            // Connection resumed successfully, the close handler can now exit.
            InvokeLog(new LogMessage(nameof(ConnectionStage), $"Reconnected, socket status: {Socket.State}", LogSeverity.Warning));
        }

        /// <summary>
        ///     Send a resume payload to the gateway.
        /// </summary>
        /// <returns></returns>
        internal Task ResumeAsync()
        {
            var resume = new SendPayload
            {
                op = OpCode.Resume,
                d = new
                {
                    token = RestClient.Config.Token,
                    session_id = Session.SessionId,
                    seq = Session.Seq
                }
            }.Serialize();

            InvokeLog(new LogMessage(nameof(ConnectionStage),
                $"Attempting to resume with seq at {Session.Seq} and session_id {Session.SessionId}",
                LogSeverity.Verbose));

            return Socket.SendMessageAsync(resume);
        }

        /// <summary>
        ///     Reset cache and send an identify payload to the gateway.
        /// </summary>
        /// <returns></returns>
        internal Task IdentifyAsync()
        {
            Cache = new FractumCache(this);
            var identify = new SendPayload
            {
                op = OpCode.Identify,
                d = new
                {
                    token = Cache.Client.RestClient.Config.Token,
                    properties = new Dictionary<string, string>
                    {
                        {"$os", Environment.OSVersion.ToString()},
                        {"$browser", Assembly.GetExecutingAssembly().FullName},
                        {"$device", Assembly.GetExecutingAssembly().FullName}
                    },
                    compress = false,
                    large_threshold = Cache.Client.RestClient.Config.LargeThreshold
                }
            }.Serialize();

            Session.Duration = DateTimeOffset.UtcNow;

            InvokeLog(new LogMessage(nameof(ConnectionStage), "Identifying", LogSeverity.Verbose));

            return Socket.SendMessageAsync(identify);
        }

        /// <summary>
        ///     Send a heartbeat to the gateway.
        /// </summary>
        /// <returns></returns>
        internal Task HeartbeatAsync()
        {
            if (Session.WaitingForACK)
            {
                HeartbeatTimer.Dispose();
                return Socket.DisconnectAsync();
            }

            var heartbeat = new SendPayload
            {
                op = OpCode.Heartbeat,
                d = Session.Seq
            }.Serialize();
            Session.WaitingForACK = true;
            LastSentHeartbeat = DateTimeOffset.UtcNow;

            InvokeLog(new LogMessage(nameof(ConnectionStage), "Heartbeat", LogSeverity.Debug));

            return Socket.SendMessageAsync(heartbeat);
        }

        #endregion

        #region Events

        internal void InvokeLog(LogMessage message)
        {
            if (RestClient.Config.DisableLogging)
                return;

            _ = LogFunc?.Invoke(message);
            _ = Log?.Invoke(message);
        }

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

        #endregion
    }
}