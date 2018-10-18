using System.Net.Http;
using System.Net.Http.Headers;
using Fractum.Rest.Utils;

namespace Fractum.Rest
{
    internal sealed class StringRestRequest : RestRequest
    {
        public StringRestRequest(RouteBuilder rb, HttpMethod verb, ulong majorParam = 0, string content = null) : base(
            rb, verb, majorParam)
        {
            if (content != null)
                Content = new StringContent(content);
        }

        public StringContent Content { get; set; }

        public override HttpRequestMessage BuildRequest()
        {
            var message = new HttpRequestMessage
            {
                RequestUri = Url,
                Method = Method,
                Content = Content
            };

            if (message.Content != null)
                message.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return message;
        }
    }
}