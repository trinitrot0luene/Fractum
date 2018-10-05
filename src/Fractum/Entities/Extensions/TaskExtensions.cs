using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Fractum.Utilities;

namespace Fractum.Entities.Extensions
{
    public static class TaskExtensions
    {
        public static async Task<T> WithCancellation<T>(
            this Task<T> task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(
                s => ((TaskCompletionSource<bool>) s).TrySetResult(true), tcs))
            {
                if (task != await Task.WhenAny(task, tcs.Task))
                    throw new OperationCanceledException(cancellationToken);
            }

            return await task;
        }

        public static async Task WithCancellation(this Task task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(
                s => ((TaskCompletionSource<bool>) s).TrySetResult(true), tcs))
            {
                if (task != await Task.WhenAny(task, tcs.Task))
                    throw new OperationCanceledException(cancellationToken);
            }

            await task;
        }

        public static IDisposable CreateTimeoutScope(this ClientWebSocket socket, TimeSpan timeSpan)
        {
            var cancellationTokenSource = new CancellationTokenSource(timeSpan);
            var cancellationTokenRegistration = cancellationTokenSource.Token.Register(socket.Abort);
            return new DisposableScope(
                () =>
                {
                    cancellationTokenRegistration.Dispose();
                    cancellationTokenSource.Dispose();
                    if (socket.State != WebSocketState.Open)
                        socket.Abort();
                });
        }
    }
}