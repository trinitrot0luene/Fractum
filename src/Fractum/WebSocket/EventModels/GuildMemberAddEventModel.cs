using System;
using Fractum.Entities;
using Newtonsoft.Json;

namespace Fractum.WebSocket.EventModels
{
    public class GuildMemberAddEventModel : EventModelBase
    {
        [JsonProperty("guild_id")]
        public ulong? GuildId { get; private set; }

        [JsonProperty("roles")]
        public ulong[] RoleIds { get; private set; }

        [JsonProperty("nick")]
        public string Nickname { get; private set; }

        [JsonProperty("mute")]
        public bool IsMuted { get; private set; }

        [JsonProperty("joined_at")]
        public DateTimeOffset JoinedAt { get; private set; }

        [JsonProperty("deaf")]
        public bool IsDeafened { get; private set; }

        [JsonProperty("user")]
        public User User { get; private set; }
    }
}