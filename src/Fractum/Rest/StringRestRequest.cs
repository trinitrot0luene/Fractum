using Fractum.Rest.Utils;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Fractum.Rest
{
    public sealed class StringRestRequest : RestRequest
    {
        public StringContent Content { get; set; }

        public StringRestRequest(RouteBuilder rb, HttpMethod verb, ulong majorParam = 0, string content = null) : base(rb, verb, majorParam)
        {
            if (content != null)
                Content = new StringContent(content);
        }

        public override HttpRequestMessage BuildRequest()
        {
            var message = new HttpRequestMessage()
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
