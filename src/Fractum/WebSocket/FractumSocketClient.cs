using System;
using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.Entities.Extensions;
using Fractum.Entities.WebSocket;
using Fractum.Rest;
using Fractum.Utilities;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.Hooks;
using Fractum.WebSocket.Pipelines;

namespace Fractum.WebSocket
{
    public sealed class FractumSocketClient : FractumRestClient
    {
        private IPipeline<Payload> _pipeline;
        internal FractumCache Cache;
        internal ISession Session;
        internal SocketWrapper Socket;

        public FractumSocketClient(FractumConfig config) : base(config)
        {
            Cache = new FractumCache(this);
            Session = new Session();
        }

        private void UseDefaultPipeline()
        {
            var connectionStage = new ConnectionStage(this);

            var eventStage = new EventStage(this)
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

            _pipeline = new PayloadPipeline()
                .AddStage(connectionStage)
                .AddStage(eventStage);
        }

        public async Task InitialiseAsync()
        {
            var gatewayInfo = await GetSocketUrlAsync();
            if (gatewayInfo.SessionStartLimit["remaining"] <= 0)
                throw new InvalidOperationException("No new sessions can be started.");

            Socket = new SocketWrapper(new Uri(gatewayInfo.Url + Consts.GATEWAY_PARAMS));

            if (_pipeline is null)
                UseDefaultPipeline();

            Socket.PayloadReceived += async payload =>
            {
                var logMessage = await _pipeline.CompleteAsync(payload);
                if (logMessage != null)
                    InvokeLog(logMessage);
            };
        }

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

        public Task ConnectAsync()
            => Socket.ConnectAsync();

        internal void InvokeLog(LogMessage msg)
            => Log?.Invoke(msg);

        internal void InvokeMessagePinned(TextChannel channel)
            => MessagePinned?.Invoke(channel);

        internal void InvokeGuildCreated(Guild guild)
            => GuildCreated?.Invoke(guild);

        internal void InvokeGuildUpdated(Guild guild)
            => GuildUpdated?.Invoke(guild);

        internal void InvokeMessageCreated(Message messsage)
            => MessageCreated?.Invoke(messsage);

        internal void InvokeChannelCreated(GuildChannel channel)
            => ChannelCreated?.Invoke(channel);

        internal void InvokeChannelUpdated(CachedEntity<GuildChannel> oldChannel, GuildChannel channel)
            => ChannelUpdated?.Invoke(oldChannel, channel);

        internal void InvokeChannelDeleted(CachedEntity<GuildChannel> channel)
            => ChannelDeleted?.Invoke(channel);

        /// <summary>
        ///     Raised when the client receives a log event.
        /// </summary>
        public event Func<LogMessage, Task> Log;

        /// <summary>
        ///     Raised when a message is pinned in a text channel.
        /// </summary>
        public event Func<TextChannel, Task> MessagePinned;

        /// <summary>
        ///     Raised when a guild becomes available to the client.
        /// </summary>
        public event Func<Guild, Task> GuildCreated;

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
    }
}