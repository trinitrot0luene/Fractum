using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.Rest.Compliance;
using Fractum.WebSocket;

namespace Fractum.Rest
{
    internal sealed class FractumRestService
    {
        private readonly ConcurrentDictionary<string, RatelimitInfo> _buckets;

        private readonly HttpClient _http;

        private readonly SemaphoreSlim _requestLock;

        private readonly FractumConfig Config;

        internal FractumRestService(FractumConfig config)
        {
            Config = config;
            _buckets = new ConcurrentDictionary<string, RatelimitInfo>();
            _http = new HttpClient();
            _requestLock = new SemaphoreSlim(1, 1);

            _http.DefaultRequestHeaders.Add("User-Agent", $"DiscordBot ({Consts.LIB_VERSION}, {Consts.GH_URL})");
            _http.DefaultRequestHeaders.Add("Authorization", $"Bot {Config.Token}");
        }

        internal async Task<HttpResponseMessage> SendRequestAsync(RestRequest request)
        {
            await _requestLock.WaitAsync();
            // If there is a bucket with no uses for this route that hasn't expired, wait until it expires.
            if (_buckets.TryGetValue(request.BucketId, out var bucket) && bucket.Remaining <= 0 &&
                bucket.ExpiresAt > DateTimeOffset.UtcNow + bucket.Offset)
            {
                InvokeLog(new LogMessage(nameof(FractumRestClient), $"Triggered pre-emptive ratelimit with delay {bucket.RequiredDelay.TotalMilliseconds}ms", LogSeverity.Warning));
                await Task.Delay(bucket.RequiredDelay);
            }

            // Send the request
            HttpResponseMessage response;
            try
            {
                InvokeLog(new LogMessage(nameof(FractumRestClient), request.BucketId.ToString(), LogSeverity.Verbose));
                response = await _http.SendAsync(request.BuildRequest());
            }
            catch (HttpRequestException httpex)
            {
                InvokeLog(new LogMessage(nameof(FractumRestClient), "An exception was thrown by the HttpClient", LogSeverity.Error, httpex));
                return default;
            }

            // Determine reaction by response code.
            switch ((int) response.StatusCode)
            {
                case 200: // OK
                case 201:
                case 204:
                case 304:
                    HandleSuccessResponse(response, request.BucketId); // Update with new bucket.
                    _requestLock.Release();
                    return response;
                case 401: // Not Allowed/Missing Permissions
                case 403:
                    throw new NotAllowedException(await response.Content.ReadAsStringAsync());
                case 400: // Bad Method/Bad Request
                case 405:
                    throw new BadRequestException(await response.Content.ReadAsStringAsync());
                case 404: // Not Found
                    throw new NotFoundException(await response.Content.ReadAsStringAsync());
                case 429: // Too Many Requests
                    var tcs = new TaskCompletionSource<HttpResponseMessage>();
                    _ = Handle429(request, response, tcs, request.BucketId);
                    _requestLock.Release();
                    response = await tcs.Task;
                    return response;
            }

            return null;
        }

        private void HandleSuccessResponse(HttpResponseMessage response, string bucketId)
        {
            var newBucket = RatelimitInfo.CreateFromResponse(response);
            if (newBucket is null) // Global Ratelimit
                return;
            _buckets.AddOrUpdate(bucketId, newBucket.Value, (k, b) => newBucket.Value);
        }

        private async Task Handle429(RestRequest request, HttpResponseMessage response,
            TaskCompletionSource<HttpResponseMessage> tcs, string bucketId)
        {
            InvokeLog(new LogMessage(nameof(FractumRestClient), "Ratelimited! Handling...", LogSeverity.Warning));

            var global = response.Headers.TryGetValues("X-RateLimit-Global", out _);
            response.Headers.TryGetValues("Retry-After", out var retry_vals);

            if (int.TryParse(string.Join("", retry_vals), out var retry_in))
                await Task.Delay(retry_in);

            await SendRequestAsync(request).ContinueWith(task => tcs.SetResult(task.Result));
        }

        internal void InvokeLog(LogMessage msg)
            => Log?.Invoke(msg);

        /// <summary>
        ///     Raised when the client receives a log event.
        /// </summary>
        public event Func<LogMessage, Task> Log;
    }
}