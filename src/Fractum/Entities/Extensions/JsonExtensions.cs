using System;
using Newtonsoft.Json;

namespace Fractum.Entities.Extensions
{
    public static class JsonExtensions
    {
        public static string Serialize(this object obj) => JsonConvert.SerializeObject(obj);

        public static T Deserialize<T>(this string value)
        {
            if (value is null) return default;

            try
            {
                var obj = JsonConvert.DeserializeObject<T>(value);
                return obj;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed deserialization: {ex.Message}");
            }
        }
    }
}