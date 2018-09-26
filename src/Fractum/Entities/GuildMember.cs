using Fractum.Entities.Contracts;
using Fractum.Rest;
using Fractum.WebSocket.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Fractum.Entities
{
    public sealed class GuildMember : FractumEntity, IUser
    {
        internal GuildMember() { }

        [JsonProperty("user")]
        internal User User { get; set; }

        [JsonIgnore]
        public Presence Presence { get; internal set; }

        [JsonIgnore]
        public Guild Guild { get; internal set; }

        [JsonIgnore]
        public ulong Id { get => User.Id; }

        [JsonIgnore]
        public DateTimeOffset CreatedAt { get => User.CreatedAt; }

        [JsonIgnore]
        public string Username { get => User.Username; }

        [JsonIgnore]
        public short Discrim { get => User.Discrim; }

        [JsonIgnore]
        public bool IsBot { get => User.IsBot; }
        
        [JsonProperty("roles")]
        internal ulong[] RoleIds { get; set; }

        [JsonIgnore]
        public ReadOnlyCollection<Role> Roles { get; internal set; }

        [JsonProperty("nick")]
        public string Nickname { get; internal set; }

        [JsonProperty("mute")]
        public bool IsMuted { get; internal set; }

        [JsonProperty("joined_at")]
        public DateTimeOffset JoinedAt { get; private set; }

        [JsonProperty("deaf")]
        public bool IsDeafened { get; internal set; }

        public string GetAvatarUrl() => User.GetAvatarUrl();
    }
}
