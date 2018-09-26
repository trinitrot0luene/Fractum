using Fractum.Rest.Compliance;
using Fractum.Rest.Utils;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Fractum.Rest
{
    public abstract class RestRequest
    {
        public Uri Url { get; set; }

        public HttpMethod Method { get; set; }

        public string BucketId { get; set; }

        public RestRequest(RouteBuilder rb, HttpMethod method, ulong majorParam = 0)
        {
            Url = rb.Build();
            Method = method;
            BucketId = RatelimitInfo.GenerateId(method, rb.GetBucketRoute(), majorParam);
        }

        public abstract HttpRequestMessage BuildRequest();
    }
}
