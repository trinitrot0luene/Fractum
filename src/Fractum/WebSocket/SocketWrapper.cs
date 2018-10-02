using Fractum.Entities;
using Fractum.Entities.Extensions;
using Fractum.WebSocket.Entities;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Collections.Concurrent;
using Fractum.WebSocket.Pipelines;

namespace Fractum.WebSocket
{
    public sealed class SocketWrapper
    {
        private const int _bufferSize = 4096;

        internal Task ListenerTask;

        private Uri _url;
        private ClientWebSocket _socket;
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private WebSocketMessageConverter _converter;

        private SemaphoreSlim _ratelimitLock;
        private DateTimeOffset _ratelimitResetsAt;
        private int _remainingMessages;

        /// <summary>
        /// Wrapper around the ClientWebSocket handling the socket connection.
        /// </summary>
        /// <param name="url">Gateway URL</param>
        internal SocketWrapper(Uri url)
        {
            _url = url;
            _socket = new ClientWebSocket();
            _converter = new WebSocketMessageConverter();
            _ratelimitLock = new SemaphoreSlim(1, 1);
            _remainingMessages = 60;
            _ratelimitResetsAt = DateTimeOffset.UtcNow.AddSeconds(60);
        }

        public void UpdateUrl(Uri url)
            => _url = url;

        /// <summary>
        /// Connect to the gateway and initiate the handshake.
        /// </summary>
        /// <returns></returns>
        public async Task ConnectAsync()
        {
            await _socket.ConnectAsync(_url, _cts.Token);
            InvokeConnected();

            ListenerTask = Task.Run(() => ListenAsync().ContinueWith(task => { if (task.IsCanceled) InvokeLog(new LogMessage(nameof(SocketWrapper), 
                "The listener task was cancelled.", LogSeverity.Error)); }), _cts.Token);
        }

        public Task DisconnectAsync(WebSocketCloseStatus status = WebSocketCloseStatus.Empty, string reason = "")
            => _socket.CloseAsync(status, reason, _cts.Token);

        /// <summary>
        /// Send a message to the gateway over a socket connection.
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
        /// Block and listen for messages/events on the socket.
        /// </summary>
        /// <returns></returns>
        private async Task ListenAsync()
        {
            var buffer = new byte[32768];
            var bufferSegment = new ArraySegment<byte>(buffer);

            byte[] resultBuffer = null;
            WebSocketReceiveResult result = null;

            var token = _cts.Token;

            while(!token.IsCancellationRequested && _socket.State == WebSocketState.Open)
            {
                using (var ms = new MemoryStream())
                {
                    do
                    {
                        result = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            // Pass close information out of the loop.
                        }
                        else
                            ms.Write(buffer, 0, result.Count);
                    }
                    while (!result.EndOfMessage);

                    resultBuffer = ms.ToArray();
                }

                string responseText = default;
                if (result.MessageType == WebSocketMessageType.Binary)
                    responseText = await _converter.DecompressAsync(resultBuffer);

                try
                {
                    _ = Task.Run(() => InvokeReceived(responseText.Deserialize<Payload>()));
                }
                catch (Exception ex)
                {
                    InvokeLog(new LogMessage(nameof(FractumSocketClient), 
                        "An exception was raised by a PayloadReceived handler.",
                        LogSeverity.Warning, ex));
                }
            }
            InvokeCloseCodeIssued(_socket.CloseStatus.Value);
        }

        #region Events

        /// <summary>
        /// Raised when the wrapper generates a LogMessage.
        /// </summary>
        public event Func<LogMessage, Task> Log;
        private void InvokeLog(LogMessage msg)
            => Log?.Invoke(msg);

        /// <summary>
        /// Raised when the client makes a successful connection to the gateway.
        /// </summary>
        public event Func<Task> OnConnected;
        private void InvokeConnected()
            => OnConnected?.Invoke();

        /// <summary>
        /// Raised when the listen loop is broken by a server disconnect.
        /// </summary>
        internal event Func<WebSocketCloseStatus, Task> ConnectionClosed;
        private void InvokeCloseCodeIssued(WebSocketCloseStatus status)
            => ConnectionClosed?.Invoke(status);

        /// <summary>
        /// Raised when the wrapper receives a payload from the gateway.
        /// </summary>
        internal event Func<Payload, Task> PayloadReceived;
        private void InvokeReceived(Payload payload)
            => PayloadReceived?.Invoke(payload);

        #endregion
    }
}
