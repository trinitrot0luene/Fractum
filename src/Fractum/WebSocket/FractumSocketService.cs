namespace Fractum.WebSocket
{
    /*
    public class FractumSocketService
    {
        internal FractumRestClient RestClient;

        private protected SocketWrapper _socket;

        private protected FractumSocketConfig _config;

        private protected SocketCache _cache;

        protected Presence _status;

        private CancellationTokenSource _connectionCts;

        private string _sessionId;

        private int _heartbeatInterval;

        private int? _seq;

        private bool _waitingForACK;

        private bool _reconnecting;

        internal FractumSocketService(FractumSocketConfig config)
        {
            RestClient = new FractumRestClient(config);
            _config = config;
            _cache = new SocketCache(RestClient);
            _status = new Presence();
        }

        public async Task ConnectAsync()
        {
            var resp = await RestClient.GetSocketUrlAsync();

            if (resp.SessionStartLimit["remaining"] == 0)
                throw new GatewayException("No new sessions can be started.");

            _socket = new SocketWrapper(new Uri(resp.Url + Consts.GATEWAY_PARAMS));

            _socket.Log += Log;
            _socket.PayloadReceived += PayloadReceivedAsync;
            _socket.ConnectionClosed += ConnectionClosedAsync;

            await _socket.ConnectAsync();
        }

        private async Task ConnectionClosedAsync(WebSocketCloseStatus status)
        {
            switch ((int)status)
            {
                case 1000:
                case 4006:
                    InvokeLog(new LogMessage(nameof(FractumSocketClient), $"Connection aborted: Socket issued close code {status}. Not attempting reconnection", LogSeverity.Error));
                    break;
                default:
                    InvokeLog(new LogMessage(nameof(FractumSocketClient), $"Connection closed with code {status}. Attempting to resume.", LogSeverity.Warning));
                    break;
            }
        }
        /*
        private async Task PayloadReceivedAsync(Payload payload)
        {
            if (payload is null)
            {
                Console.WriteLine("NULL");
                return;
            }

            switch(payload.OpCode)
            {
                case OpCode.Hello:
                    _heartbeatInterval = payload.DataObject.Value<int>("heartbeat_interval");
                    _socket.HeartbeatTimer = new Timer((_)
                        => Task.Run(() => _socket.HeartbeatAsync()), null, _heartbeatInterval, _heartbeatInterval);
                    await IdentifyAsync();
                    break;
                case OpCode.HeartbeatACK:
                    _waitingForACK = false;
                    InvokeLog(new LogMessage(nameof(FractumSocketClient), "Heartbeat ACK", LogSeverity.Debug));
                    break;
                case OpCode.InvalidSession:
                    await _socket;
                    await Task.Delay(new Random().Next(1, 6) * 1000);
                    
                    if (payload.Data.ToObject<bool>() && _sessionId != null && _seq != null)
                        await ResumeAsync();
                    else
                    {
                        _cache = new SocketCache(RestClient);
                        await IdentifyAsync();
                    }
                    break;
                case OpCode.Reconnect:
                    await _socket.;
                    await ConnectAsync();
                    break;
                case OpCode.Dispatch:
                    if (payload.Seq.HasValue)
                        _seq = payload.Seq;
                    switch(payload.Type)
                    {
                        #region Meta Updates
                        case "READY":
                            InvokeReady();
                            break;
                        #endregion
                        #region Guild Updates
                        case "GUILD_CREATE":
                            var newGuild = payload.Data.ToObject<Guild>();
                            _cache.Add(newGuild);
                            InvokeCreated(newGuild);
                            break;
                        case "GUILD_MEMBERS_CHUNK":
                            var eventData = payload.Data.ToObject<GuildMemberChunkEvent>();
                            if (!_cache.TryUpdateMembers(eventData.GuildId, eventData.Members))
                                InvokeLog(new LogMessage(nameof(FractumSocketClient), $"Failed to add member chunk to cache for id {eventData.GuildId}", LogSeverity.Warning));
                            break;
                        #endregion
                        case "MESSAGE_CREATE":
                            var message = payload.Data.ToObject<Message>();
                            _cache.PopulateMessage(message);
                            InvokeMessageReceived(message);
                            break;
                        #region User Updates
                        case "PRESENCE_UPDATE":
                            var presence = payload.Data.ToObject<PresenceUpdateEvent>();
                            if (presence.GuildId.HasValue)
                                _cache.UpdateStatus(presence);
                            break;
                        case "GUILD_MEMBER_UPDATE":
                            var updateData = payload.Data.ToObject<PresenceUpdateEvent>();
                            if (updateData.GuildId.HasValue)
                                _cache.UpdateStatus(updateData);
                            break;
                        #endregion
                        default:
                            InvokeLog(new LogMessage(nameof(FractumSocketClient), payload.Type, LogSeverity.Info));
                            break;
                    }
                    break;
            }
        }

        public Task UpdatePresenceAsync(Presence newPresence)
            => _socket.SendMessageAsync(new
            {
                op = OpCode.StatusUpdate,
                d = newPresence
            }.Serialize());

        public Task RequestGuildMembersAsync(ulong guildId, string queryString = null, int limit = 0)
            => _socket.SendMessageAsync(new
            {
                op = OpCode.RequestGuildMembers,
                d = new
                {
                    guild_id = guildId,
                    query = queryString ?? string.Empty,
                    limit = 0
                }
            }.Serialize());

        /// <summary>
        /// Send OpCode: 6 Resume
        /// </summary>
        /// <returns></returns>
        private async Task ResumeAsync()
        {
            await _socket.ConnectAsync();

            await _socket.SendMessageAsync(new
            {
                op = OpCode.Resume,
                session_id = _sessionId,
                seq = _seq
            }.Serialize());

            InvokeLog(new LogMessage(nameof(FractumSocketClient), "Resume", LogSeverity.Info));

            _sessionId = null; // Invalidate the session we just tried to resume- if we get op9 again it'll identify again instead.
        }

        /// <summary>
        /// Send OpCode: 2 Identify
        /// </summary>
        /// <returns></returns>
        private async Task IdentifyAsync()
        {
            var identify = new Payload
            {
                OpCode = OpCode.Identify,
                Data = JToken.Parse(new
                {
                    token = _config.Token,
                    properties = new Dictionary<string, string>()
                    {
                        { "$os", Environment.OSVersion.ToString() },
                        { "$browser", Assembly.GetExecutingAssembly().FullName },
                        { "$device", Assembly.GetExecutingAssembly().FullName }
                    },
                    compress = false,
                    large_threshold = _config.LargeThreshold ?? 250,
                    shard = new[] { 0, 1 },
                    presence = _status
                }.Serialize())
            }.Serialize();

            await _socket.SendMessageAsync(identify);

            InvokeLog(new LogMessage(nameof(FractumSocketClient), "Identify", LogSeverity.Debug));
        }

        #region Events

        public event Func<Guild, Task> GuildAvailable;
        private void InvokeCreated(Guild guild)
        {
            InvokeLog(new LogMessage(nameof(FractumSocketClient), $"{guild.Name} Available", LogSeverity.Info));
            GuildAvailable?.Invoke(guild);
        }

        public event Func<Message, Task> MessageReceived;
        private void InvokeMessageReceived(Message msg)
        {
            InvokeLog(new LogMessage(nameof(FractumSocketClient), $"Create Message: {msg.Member.Nickname ?? msg.Author.Username}", LogSeverity.Verbose));
            MessageReceived?.Invoke(msg);
        }

        public event Func<Task> Ready;
        private void InvokeReady()
        {
            InvokeLog(new LogMessage(nameof(FractumSocketClient), "Ready", LogSeverity.Info));
            Ready?.Invoke();
        }

        public event Func<LogMessage, Task> Log;
        private void InvokeLog(LogMessage msg)
            => Log?.Invoke(msg);

        #endregion
    }
    */
}