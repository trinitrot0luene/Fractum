using System;
using System.Buffers;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fractum;
using Fractum.WebSocket;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket
{
    internal sealed class SocketWrapper
    {
        private const int _bufferSize = 4096;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private static ArrayPool<byte> _pool = ArrayPool<byte>.Create();

        private readonly SemaphoreSlim _ratelimitLock;
        private WebSocketMessageConverter _converter;
        private DateTimeOffset _ratelimitResetsAt;
        private int _remainingMessages;
        private ClientWebSocket _socket;
        private DateTimeOffset? _startedAt;

        private Uri _url;

        internal Task ListenerTask;
        internal TimeSpan? Uptime => _startedAt.HasValue ? DateTimeOffset.UtcNow - _startedAt : default;

        /// <summary>
        ///     Wrapper around the ClientWebSocket handling the socket connection.
        /// </summary>
        /// <param name="url">Gateway URL</param>
        internal SocketWrapper(Uri url)
        {
            _url = url;
            _converter = new WebSocketMessageConverter();
            _ratelimitLock = new SemaphoreSlim(1, 1);
            _remainingMessages = 60;
            _ratelimitResetsAt = DateTimeOffset.UtcNow.AddSeconds(60);
        }

        internal WebSocketState State => _socket.State;

        public void UpdateUrl(Uri url)
            => _url = url;

        /// <summary>
        ///     Connect to the gateway and initiate the handshake.
        /// </summary>
        /// <returns></returns>
        public async Task ConnectAsync()
        {
            _socket = new ClientWebSocket();

            var abortTask = Task.Run(async () =>
            {
                await Task.Delay(5000);
                if (_socket.State != WebSocketState.Open)
                    _socket.Abort();
            });
            await Task.WhenAny(abortTask, _socket.ConnectAsync(_url, _cts.Token));

            if (_socket.State != WebSocketState.Open)
                return;

            InvokeConnected();

            InvokeLog(new LogMessage(nameof(SocketWrapper), "Spawning new listener task", LogSeverity.Debug));

            ListenerTask = Task.Run(() => ListenAsync(), _cts.Token).ContinueWith(task =>
            {
                if (task.IsCanceled)
                    InvokeLog(new LogMessage(nameof(SocketWrapper),
                        "The listener task was cancelled.", LogSeverity.Error));

                _startedAt = default;
            });
        }

        public async Task DisconnectAsync(WebSocketCloseStatus status = WebSocketCloseStatus.Empty, string reason = null, bool invokeCloseCodeIssued = true)
        {
            try
            {
                if (_socket.State == WebSocketState.Open)
                    await _socket.CloseAsync(status, reason, _cts.Token);
            }
            finally
            {
                _socket.Dispose();
                _socket = new ClientWebSocket();
                _converter = new WebSocketMessageConverter();

                if (invokeCloseCodeIssued)
                    InvokeCloseCodeIssued(status);
            }
        }

        /// <summary>
        ///     Send a message to the gateway over a socket connection.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendMessageAsync(string message)
        {
            if (DateTimeOffset.UtcNow > _ratelimitResetsAt)
            {
                _remainingMessages = 60;
                _ratelimitResetsAt = DateTimeOffset.UtcNow.AddSeconds(60);
            }

            if (_remainingMessages <= 0 && DateTimeOffset.UtcNow < _ratelimitResetsAt)
                await Task.WhenAll(_ratelimitLock.WaitAsync(), Task.Delay(_ratelimitResetsAt - DateTimeOffset.UtcNow));
            else
                await _ratelimitLock.WaitAsync();

            Interlocked.Decrement(ref _remainingMessages);

            if (_socket.State != WebSocketState.Open)
                throw new InvalidOperationException("You cannot send messages to a disconnected socket.");

            var msgBytes = Encoding.UTF8.GetBytes(message);
            if (msgBytes.Length > _bufferSize)
                throw new InvalidOperationException($"Cannot send a payload over {_bufferSize} bytes in length.");

            await _socket.SendAsync(new ArraySegment<byte>(msgBytes), WebSocketMessageType.Text, true, _cts.Token);

            _ratelimitLock.Release();
        }

        /// <summary>
        ///     Block and listen for messages/events on the socket.
        /// </summary>
        /// <returns></returns>
        private async Task ListenAsync()
        {
            var buffer = _pool.Rent(32768);

            WebSocketReceiveResult result = null;

            var token = _cts.Token;

            while (!token.IsCancellationRequested && _socket.State == WebSocketState.Open)
            {
                byte[] resultBuffer;
                int realLen = 0;
                try
                {
                    using (var ms = new MemoryStream())
                    {
                        do
                        {
                            result = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);
                            if (result.MessageType == WebSocketMessageType.Close)
                                break;
                            else
                                ms.Write(buffer, 0, result.Count);
                        }
                        while (!result.EndOfMessage);

                        resultBuffer = _pool.Rent((int)ms.Length);
                        ms.Position = 0;
                        ms.Read(resultBuffer, 0, (int)ms.Length);

                        realLen = (int)ms.Length;
                    }
                }
                catch (WebSocketException wsexception)
                {
                    InvokeCloseCodeIssued(WebSocketCloseStatus.Empty, wsexception.Message);

                    _socket.Dispose();
                    _socket = new ClientWebSocket();
                    _converter = new WebSocketMessageConverter();

                    break;
                }

                if (result.MessageType != WebSocketMessageType.Close)
                {
                    (Payload rawPayload, IPayload<EventModelBase> matchedPayload) responsePayload = default;
                    if (result.MessageType == WebSocketMessageType.Binary)
                        responsePayload = await _converter.DecompressAsync(new ArraySegment<byte>(resultBuffer, 0, realLen));
                    _pool.Return(resultBuffer);
                    try
                    {
                        if (responsePayload.matchedPayload == null)
                            InvokeLog(new LogMessage(nameof(SocketWrapper), $"Unknown t: {responsePayload.rawPayload.Type}", LogSeverity.Debug));
                        else
                        {
                            if (responsePayload.matchedPayload.Data != null)
                                responsePayload.matchedPayload.Data.ProcessingStartedAt = DateTimeOffset.UtcNow;

                            var cts = new CancellationTokenSource();
                            var warningTask = Task.Delay(3000, cts.Token).ContinueWith(task =>
                            {
                                if (!task.IsCanceled)
                                    InvokeLog(new LogMessage("Gateway",
                                        "The gateway connection is being blocked by an event handler. Ensure your handlers do not run for longer than 3 seconds or wrap them in an unawaited Task.Run!", 
                                        LogSeverity.Warning));
                            });
                            var handlerTask = PayloadReceived?.Invoke(responsePayload.matchedPayload).ContinueWith(task =>
                            {
                                if (!warningTask.IsCompleted)
                                    cts.Cancel();
                            });
                            await Task.WhenAll(warningTask, handlerTask);
                        }
                    }
                    catch (Exception ex)
                    {
                        InvokeLog(new LogMessage(nameof(SocketWrapper),
                            "An exception was raised by a PayloadReceived handler.",
                            LogSeverity.Warning, ex));
                    }
                }
            }
            _pool.Return(buffer);

            InvokeCloseCodeIssued(_socket.CloseStatus ?? WebSocketCloseStatus.Empty, result.CloseStatusDescription);

            _socket.Dispose();
            _socket = new ClientWebSocket();
            _converter = new WebSocketMessageConverter();
        }

        #region Events

        /// <summary>
        ///     Raised when the wrapper generates a LogMessage.
        /// </summary>
        public event Func<LogMessage, Task> OnLog;

        private void InvokeLog(LogMessage msg)
            => OnLog?.Invoke(msg);

        /// <summary>
        ///     Raised when the client makes a successful connection to the gateway.
        /// </summary>
        public event Func<Task> OnConnected;

        private void InvokeConnected()
        {
            _startedAt = DateTimeOffset.UtcNow;
            OnConnected?.Invoke();
        }

        /// <summary>
        ///     Raised when the listen loop is broken by a server disconnect.
        /// </summary>
        internal event Func<WebSocketCloseStatus, string, Task> ConnectionClosed;

        private void InvokeCloseCodeIssued(WebSocketCloseStatus status, string message = null)
        {
            _startedAt = null;
            ConnectionClosed?.Invoke(status, message);
        }

        /// <summary>
        ///     Raised when the wrapper receives a payload from the gateway.
        /// </summary>
        internal event Func<IPayload<EventModelBase>, Task> PayloadReceived;

        private void InvokeReceived(IPayload<EventModelBase> payload)
            => PayloadReceived?.Invoke(payload);

        #endregion
    }
}