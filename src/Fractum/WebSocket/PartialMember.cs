﻿using System;
using Newtonsoft.Json;

namespace Fractum.WebSocket
{
    public sealed class PartialMember
    {
        internal PartialMember()
        {
        }

        [JsonProperty("roles")]
        public ulong[] RoleIds { get; private set; }

        [JsonProperty("nick")]
        public string Nickname { get; private set; }

        [JsonProperty("mute")]
        public bool? IsMuted { get; private set; }

        [JsonProperty("deaf")]
        public bool? IsDeafened { get; private set; }

        [JsonProperty("joined_at")]
        public DateTimeOffset? JoinedAt { get; private set; }
    }
}