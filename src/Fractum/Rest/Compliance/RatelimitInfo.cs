using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Fractum.Rest.Compliance
{
    /// <summary>
    /// Caches ratelimit information for routes on the REST API.
    /// </summary>
    internal struct RatelimitInfo
    {
        public int Limit { get; set; }

        public int Remaining { get; set; }

        public DateTimeOffset ExpiresAt { get; set; }

        public TimeSpan Offset { get; set; }

        public TimeSpan RequiredDelay => ExpiresAt - (DateTimeOffset.UtcNow.Add(Offset));

        private RatelimitInfo(HttpResponseMessage msg)
        {
            if (msg.Headers.TryGetValues("X-RateLimit-Limit", out var limit_vals)
                && int.TryParse(string.Join("", limit_vals), out int limit))
                Limit = limit;
            else Limit = -1;

            if (msg.Headers.TryGetValues("X-RateLimit-Remaining", out var remaining_vals)
                && int.TryParse(string.Join("", remaining_vals), out int remaining))
                Remaining = remaining;
            else
                Remaining = -1;

            if (msg.Headers.TryGetValues("X-RateLimit-Reset", out var reset_vals)
                && int.TryParse(string.Join("", reset_vals), out int reset)
                && msg.Headers.TryGetValues("Date", out var issue_vals)
                && DateTimeOffset.TryParse(string.Join("", issue_vals), out var issuedAt))
            {
                ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(reset);
                Offset = issuedAt - DateTimeOffset.UtcNow;
            }
            else
            {
                ExpiresAt = DateTimeOffset.UtcNow;
                Offset = TimeSpan.Zero;
            }
        }

        public static string GenerateId(HttpMethod verb, string route, ulong majorParameter = 0)
            => $"{verb.Method} | {majorParameter} | {route}";

        internal static RatelimitInfo? CreateFromResponse(HttpResponseMessage msg)
        {
            var info = new RatelimitInfo(msg);
            if (info.Limit == -1)
                return null;
            return info;
        }
    }
}
