using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fractum.Entities.Extensions
{
    public static class JsonExtensions
    {
        public static string Serialize(this object obj) => JsonConvert.SerializeObject(obj);

        public static T Deserialize<T>(this string value)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject<T>(value);
                return obj;
            }
            catch(JsonException jsonEx) { throw new Exception($"Failed deserialization: {jsonEx.Message}"); };
        }
    }
}
