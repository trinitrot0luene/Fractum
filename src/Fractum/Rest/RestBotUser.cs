using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fractum.Rest
{
    public sealed class RestBotUser : RestUser
    {
        [JsonProperty("verified")]
        public bool IsVerified { get; private set; }

        [JsonProperty("locale")]
        public string Locale { get; private set; }

        [JsonProperty("flags")]
        public int Flags { get; private set; }

        [JsonProperty("email")]
        public string Email { get; private set; }
    }
}
