﻿using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Fractum;
using Fractum.WebSocket;
using Fractum.Extensions;
using Fractum.Rest;
using Fractum.WebSocket.EventModels;
using Fractum.WebSocket.Hooks;
using Microsoft.Extensions.DependencyInjection;

namespace Fractum.WebSocket
{
    public sealed class GatewayClient
    {
        #region Hidden Properties & Fields

        private IPipeline<IPayload<EventModelBase>> _pipeline;

        private SemaphoreSlim _reconnectionSemaphore;

        private DateTimeOffset? _sessionStartedAt;

        internal Timer HeartbeatTimer { get; set; }

        internal GatewayCache Cache;

        internal DateTimeOffset LastSentHeartbeat;

        internal GatewaySession Session;

        internal SocketWrapper Socket;

        internal ServiceCollection PipelineServices;

        #endregion

        #region Public Properties & Fields

        public RestClient RestClient { get; private set; }

        public RestBotUser BotUser { get; private set; }

        public IKeyedEnumerable<ulong, CachedGuild> Guilds => new KeyedGuildWrapper(Cache);

        public IKeyedEnumerable<ulong, CachedGuildChannel> Channels => new KeyedChannelWrapper(Cache);

        public IKeyedEnumerable<ulong, CachedDMChannel> DMChannels => new KeyedDMChannelWrapper(Cache);

        public IKeyedEnumerable<ulong, User> Users => new KeyedUserWrapper(Cache);

        public int Latency { get; internal set; }

        public ClientStatus ClientStatus { get; internal set; }

        public TimeSpan SocketUptime => Socket.Uptime ?? TimeSpan.Zero;

        public TimeSpan SessionUptime => _sessionStartedAt.HasValue ? DateTimeOffset.UtcNow - _sessionStartedAt.Value : TimeSpan.Zero;

        #endregion

        public GatewayClient(GatewayConfig config)
        {
            _reconnectionSemaphore = new SemaphoreSlim(1, 1);

            Cache = new GatewayCache(this);
            Session = new GatewaySession();
            RestClient = new RestClient(config);

            GetOrConfigurePipeline();

            PipelineServices = new ServiceCollection();

            ClientStatus = ClientStatus.Disconnected;

            RestClient.OnLog += msg =>
            {
                InvokeLog(msg);
                return Task.CompletedTask;
            };
        }

        /// <summary>
        /// Subscribe a default log handler implementation to the client's <see cref="OnLog"/> event.
        /// </summary>
        /// <param name="minSeverity">The minimum severity required for an event to be be logged.</param>
        /// <param name="hidePresenceUpdates">Whether presence updates should be included in the log output, regardless of what the minimum severity is set to.</param>
        public void UseDefaultLogging(LogSeverity minSeverity = LogSeverity.Info, bool hidePresenceUpdates = false)
        {
            OnLog += (msg) =>
            {
                if (msg.Severity < minSeverity || (hidePresenceUpdates && msg.Source == "PresenceUpdateHook"))
                    return Task.CompletedTask;

                ConsoleColor color = ConsoleColor.DarkGray;

                switch(msg.Severity)
                {
                    case LogSeverity.Error:
                        color = ConsoleColor.Red;
                        break;
                    case LogSeverity.Warning:
                        color = ConsoleColor.DarkYellow;
                        break;
                    case LogSeverity.Info:
                        color = ConsoleColor.Green;
                        break;
                    case LogSeverity.Verbose:
                        color = ConsoleColor.Cyan;
                        break;
                }
                Console.ForegroundColor = color;

                Console.WriteLine(msg);

                return Task.CompletedTask;
            };
        }

        /// <summary>
        ///     Gets or initialises the client's default <see cref="PayloadPipeline"/>.
        /// </summary>
        public IPipeline<IPayload<EventModelBase>> GetOrConfigurePipeline()
        {
            if (_pipeline == null)
            {
                _pipeline = new PayloadPipeline();

                AddConnectionStage();
                AddEventStage();
            }

            return _pipeline;
        }

        public EventStage GetEventStage()
            => _pipeline.GetStage<EventStage>() as EventStage;

        private void AddEventStage()
        {
            _pipeline.AddStage(new EventStage(this)
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
        }

        private void AddConnectionStage()
            => _pipeline.AddStage(new ConnectionStage());

        /// <summary>
        ///     Add an <see cref="IPipelineStage{TData}"/> to the pipeline.
        /// </summary>
        /// <typeparam name="T">Type of the stage to be added</typeparam>
        /// <param name="ctorParameters">Optional parameters to be used when constructing the stage</param>
        /// <returns></returns>
        public GatewayClient AddStage<T>(params object[] ctorParameters)
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
        public GatewayClient AddStage<T>(T inst)
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

        #endregion

        /// <summary>
        ///     Performs all pre-flight checks for the client, retrieves all necessary information to make a gateway connection and prepares the client to start a session.
        /// </summary>
        /// <returns></returns>
        public async Task InitialiseAsync()
        {
            // TODO: Validate token FractumUtils.ValidateToken(Config.Token);
            
            var gatewayInfo = await RestClient.GetSocketUrlAsync().ConfigureAwait(false);
            if (gatewayInfo.SessionStartLimit["remaining"] <= 0)
                throw new InvalidOperationException("No new sessions can be started under this token.");

            BotUser = await RestClient.GetCurrentUserAsync();

            Socket = new SocketWrapper(new Uri(gatewayInfo.Url + Consts.GATEWAY_PARAMS));

            Socket.ConnectionClosed += ReconnectAsync;

            Socket.OnLog += OnLog;

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
        public Task ConnectAsync()
            =>  Socket.ConnectAsync();

        /// <summary>
        ///     Disconnect from the socket, suppressing automatic client reconnects.
        /// </summary>
        /// <returns></returns>
        public Task DisconnectAsync()
        {
            ClientStatus = ClientStatus.Disconnected;

            return Socket.DisconnectAsync(WebSocketCloseStatus.NormalClosure, null, false);
        }

        #region Socket Logic

        /// <summary>
        ///     Handle socket closures and subsequent reconnections.
        /// </summary>
        /// <param name="status">Details of the closure.</param>
        /// <returns></returns>
        internal async Task ReconnectAsync(WebSocketCloseStatus status, string message = null)
        {
            if (_reconnectionSemaphore.CurrentCount == 0)
                return;
            else
                await _reconnectionSemaphore.WaitAsync();

            ClientStatus = ClientStatus.Reconnecting;

            Session.WaitingForACK = false; // We've been disconnected, reset checks for zombied connections.
            Session.ReconnectionAttempts = 0;

            HeartbeatTimer.Dispose();

            var statusCode = (int)status;

            InvokeLog(new LogMessage(nameof(ConnectionStage),
                $"Disconnected from the gateway with error {statusCode}:{GatewayCloseCode.GetCodeName(statusCode)} {message ?? ""}",
                LogSeverity.Error));

            if (statusCode == 1000 || statusCode == 4001 || statusCode == 4006)
                Session.Invalidated = true;

            var backoffPower = 1;
            var backoff = 2;

            InvokeLog(new LogMessage(nameof(ConnectionStage), "Reconnecting...",
                LogSeverity.Warning));
            do
            {
                // Make 3 attempts to connect (and resume) then try to refetch the gateway url.
                if (Session.ReconnectionAttempts != 0 && Session.ReconnectionAttempts % 3 == 0)
                    try
                    {
                        // Fetch connection info from the gateway with new Url: 
                        var gatewayInfo = await RestClient.GetSocketUrlAsync()
                            .ContinueWith(task => !task.IsFaulted ? task.Result : default);
                        if (gatewayInfo?.SessionStartLimit["remaining"] == 0 && Session.Invalidated)
                            throw new GatewayException("No new sessions can be started");
                        if (gatewayInfo != null)
                            Session.GatewayUrl = gatewayInfo.Url;
                        Socket.UpdateUrl(new Uri(Session.GatewayUrl + Consts.GATEWAY_PARAMS));
                    }
                    // If there's no network connection this will fail, don't update with new session data.
                    catch (Exception)
                    {
                        InvokeLog(new LogMessage(nameof(ConnectionStage),
                            "Failed to update session with new gateway url", LogSeverity.Warning));
                    }

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

            _reconnectionSemaphore.Release();
            
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
            Cache = new GatewayCache(this);
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
            => _ = OnLog?.Invoke(message);

        internal void InvokeMessagePinned(ITextChannel channel)
            => _ = OnMessagePinned?.Invoke(channel);

        internal void InvokeGuildCreated(CachedGuild guild)
            => _ = OnGuildCreated?.Invoke(guild);

        internal void InvokeGuildUnavailable(CachedGuild guild)
            => _ = OnGuildUnavailable?.Invoke(guild);

        internal void InvokeGuildUpdated(CachedGuild guild)
            => _ = OnGuildUpdated?.Invoke(guild);

        internal void InvokeMessageCreated(CachedMessage message)
            => _ = OnMessageCreated?.Invoke(message);

        internal void InvokeMessageUpdated(CachedMessage oldMessage, CachedMessage newMessage)
            => _ = OnMessageUpdated?.Invoke(oldMessage, newMessage);

        internal void InvokeMessageDeleted(Cacheable<CachedMessage> cachedMessage)
            => _ = OnMessageDeleted?.Invoke(cachedMessage);

        internal void InvokeChannelCreated(CachedChannel channel)
            => _ = OnChannelCreated?.Invoke(channel);

        internal void InvokeChannelUpdated(Cacheable<CachedGuildChannel> oldChannel, CachedGuildChannel channel)
            => _ = OnChannelUpdated?.Invoke(oldChannel, channel);

        internal void InvokeChannelDeleted(Cacheable<CachedGuildChannel> channel)
            => _ = OnChannelDeleted?.Invoke(channel);

        internal void InvokeMemberBanned(CachedMember member)
            => _ = OnMemberBanned?.Invoke(member);

        internal void InvokeMemberUnbanned(User user)
            => _ = OnMemberUnbanned?.Invoke(user);

        internal void InvokeEmojisUpdated(Cacheable<CachedGuild> guild, IEnumerable<GuildEmoji> emojis)
            => _ = OnEmojisUpdated?.Invoke(guild, emojis);

        internal void InvokeIntegrationsUpdated(CachedGuild guild)
            => _ = OnIntegrationsUpdated?.Invoke(guild);

        internal void InvokeMemberJoined(CachedMember member)
            => _ = OnMemberJoined?.Invoke(member);

        internal void InvokeMemberLeft(Cacheable<CachedMember> user)
            => _ = OnMemberLeft?.Invoke(user);

        internal void InvokeRoleCreated(CachedGuild guild, Role role)
            => _ = OnRoleCreated?.Invoke(guild, role);

        internal void InvokeRoleUpdated(CachedGuild guild, Role role)
            => _ = OnRoleUpdated?.Invoke(guild, role);

        internal void InvokeRoleDeleted(CachedGuild guild, Role role)
            => _ = OnRoleDeleted?.Invoke(guild, role);

        internal void InvokeReactionAdded(CachedReaction reaction)
            => _ = OnReactionAdded?.Invoke(reaction);

        internal void InvokeReactionRemoved(CachedReaction reaction)
            => _ = OnReactionRemoved?.Invoke(reaction);

        internal void InvokeReactionsCleared(ulong messageId, ulong channelId, ulong? guildId)
            => _ = OnReactionsCleared?.Invoke(messageId, channelId, guildId);

        internal void InvokeUserUpdated(Cacheable<User> oldUser, User user)
            => _ = OnUserUpdated?.Invoke(oldUser, user);

        internal void InvokeReady()
        {
            _sessionStartedAt = DateTimeOffset.UtcNow;

            ClientStatus = ClientStatus.Connected;

            _ = OnReady?.Invoke();
        }

        /// <summary>
        ///     Raised when a message is pinned in a text channel.
        /// </summary>
        public event Func<ITextChannel, Task> OnMessagePinned;

        /// <summary>
        ///     Raised when a guild becomes available to the client.
        /// </summary>
        public event Func<CachedGuild, Task> OnGuildCreated;

        /// <summary>
        ///     Raised when a guild becomes unavailable to the client.
        /// </summary>
        public event Func<CachedGuild, Task> OnGuildUnavailable;

        /// <summary>
        ///     Raised when a guild is updated.
        /// </summary>
        public event Func<CachedGuild, Task> OnGuildUpdated;

        /// <summary>
        ///     Raised when a message is created.
        /// </summary>
        public event Func<CachedMessage, Task> OnMessageCreated;

        /// <summary>
        ///     Raised when a message is edited.
        /// </summary>
        public event Func<CachedMessage, CachedMessage, Task> OnMessageUpdated;

        /// <summary>
        ///     Raised when a message is deleted.
        /// </summary>
        public event Func<Cacheable<CachedMessage>, Task> OnMessageDeleted;

        /// <summary>
        ///     Raised when a channel is created.
        /// </summary>
        public event Func<CachedChannel, Task> OnChannelCreated;

        /// <summary>
        ///     Raised when a channel is updated.
        /// </summary>
        public event Func<Cacheable<CachedGuildChannel>, CachedGuildChannel, Task> OnChannelUpdated;

        /// <summary>
        ///     Raised when a channel is deleted.
        /// </summary>
        public event Func<Cacheable<CachedGuildChannel>, Task> OnChannelDeleted;

        /// <summary>
        ///     Raised when a member is banned.
        /// </summary>
        public event Func<CachedMember, Task> OnMemberBanned;

        /// <summary>
        ///     Raised when a member is unbanned.
        /// </summary>
        public event Func<User, Task> OnMemberUnbanned;

        /// <summary>
        ///     Raised when emojis in a guild are updated.
        /// </summary>
        public event Func<Cacheable<CachedGuild>, IEnumerable<GuildEmoji>, Task> OnEmojisUpdated;

        /// <summary>
        ///     Raised when a guild's integrations are updated.
        /// </summary>
        public event Func<CachedGuild, Task> OnIntegrationsUpdated;

        /// <summary>
        ///     Raised when a member joins a guild.
        /// </summary>
        public event Func<CachedMember, Task> OnMemberJoined;

        /// <summary>
        ///     Raised when a member leaves a guild.
        /// </summary>
        public event Func<Cacheable<CachedMember>, Task> OnMemberLeft;

        /// <summary>
        ///     Raised when a role is created.
        /// </summary>
        public event Func<CachedGuild, Role, Task> OnRoleCreated;

        /// <summary>
        ///     Raised when a role is updated.
        /// </summary>
        public event Func<CachedGuild, Role, Task> OnRoleUpdated;

        /// <summary>
        ///     Raised when a role is deleted.
        /// </summary>
        public event Func<CachedGuild, Role, Task> OnRoleDeleted;

        /// <summary>
        ///     Raised when a reaction is added to a message.
        /// </summary>
        public event Func<CachedReaction, Task> OnReactionAdded;

        /// <summary>
        ///     Raised when a reaction is removed from a message.
        /// </summary>
        public event Func<CachedReaction, Task> OnReactionRemoved;

        /// <summary>
        ///     Raised when the reactions for a message are cleared.
        /// </summary>
        public event Func<ulong, ulong, ulong?, Task> OnReactionsCleared;

        /// <summary>
        ///     Raised when a user is updated.
        /// </summary>
        public event Func<Cacheable<User>, User, Task> OnUserUpdated;

        /// <summary>
        ///     Raised when the client has successfully identified and is ready to receive and send events and commands.
        /// </summary>
        public event Func<Task> OnReady;

        /// <summary>
        ///     Raised by the client when log-worthy events are encountered.
        /// </summary>
        public event Func<LogMessage, Task> OnLog;

        #endregion
    }
}