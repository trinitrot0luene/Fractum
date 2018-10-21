using System;
using Fractum.Entities;
using Newtonsoft.Json;

namespace Fractum.WebSocket.EventModels
{
    public class GuildMemberAddEventModel : EventModelBase
    {
        [JsonProperty("guild_id")]
        internal ulong? GuildId { get; private set; }

        [JsonProperty("roles")]
        internal ulong[] RoleIds { get; set; }

        [JsonProperty("nick")]
        public string Nickname { get; internal set; }

        [JsonProperty("mute")]
        public bool IsMuted { get; internal set; }

        [JsonProperty("joined_at")]
        public DateTimeOffset JoinedAt { get; private set; }

        [JsonProperty("deaf")]
        public bool IsDeafened { get; internal set; }

        [JsonProperty("user")]
        public User User { get; private set; }
    }
}