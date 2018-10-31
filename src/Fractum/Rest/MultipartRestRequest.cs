using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Fractum.Rest.Utils;

namespace Fractum.Rest
{
    internal sealed class MultipartRestRequest : RestRequest
    {
        private readonly MultipartFormDataContent _content = new MultipartFormDataContent("------------------");

        public MultipartRestRequest(RouteBuilder rb, HttpMethod method, Dictionary<string, string> multipartValues,
            Dictionary<string, (string fileName, Stream fileStream)> attachments, ulong majorParam = 0) : base(rb,
            method, majorParam)
        {
            foreach (var kvp in multipartValues)
            {
                var content = new StringContent(kvp.Value, Encoding.UTF8);

                _content.Add(content, kvp.Key);
            }

            foreach (var kvp in attachments)
                _content.Add(new StreamContent(kvp.Value.fileStream), kvp.Key, kvp.Value.fileName);
        }

        public override HttpRequestMessage BuildRequest()
        {
            var message = new HttpRequestMessage
            {
                RequestUri = Url,
                Method = Method,
                Content = _content
            };

            return message;
        }
    }
}