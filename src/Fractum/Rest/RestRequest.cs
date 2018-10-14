using System;
using System.Net.Http;
using Fractum.Rest.Compliance;
using Fractum.Rest.Utils;

namespace Fractum.Rest
{
    internal abstract class RestRequest
    {
        public RestRequest(RouteBuilder rb, HttpMethod method, ulong majorParam = 0)
        {
            Url = rb.Build();
            Method = method;
            BucketId = RatelimitInfo.GenerateId(method, rb.GetBucketRoute(), majorParam);
        }

        public Uri Url { get; set; }

        public HttpMethod Method { get; set; }

        public string BucketId { get; set; }

        public abstract HttpRequestMessage BuildRequest();
    }
}