using Fractum.Entities.Rest;
using Fractum.Rest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Fractum.Extensions
{
    internal static class HttpExtensions
    {
        public static async Task<T> ReadAsObjectAsync<T>(this HttpContent content, FractumRestClient client) where T : RestEntity
        {
            var rawContent = await content.ReadAsStringAsync();

            var entity = JsonConvert.DeserializeObject<T>(rawContent);

            entity.Client = client;

            return entity;
        }

        public static async Task<T> ReadAsObjectAsync<T>(this HttpContent content)
        {
            var rawContent = await content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(rawContent);
        }
    }
}
