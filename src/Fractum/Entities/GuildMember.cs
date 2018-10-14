using System;
using System.Collections.ObjectModel;
using Fractum.Contracts;
using Fractum.Entities.WebSocket;
using Newtonsoft.Json;

namespace Fractum.Entities
{
    public sealed class GuildMember : FractumEntity, IUser
    {
        internal GuildMember()
        {
        }

        [JsonProperty("user")]
        internal User User { get; set; }

        [JsonIgnore]
        public Presence Presence { get; internal set; }

        [JsonIgnore]
        public Guild Guild { get; internal set; }

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

        [JsonIgnore]
        public string Mention
        {
            get => string.Format(Consts.USER_MENTION, Id);
        }

        [JsonIgnore]
        public ulong Id => User.Id;

        [JsonIgnore]
        public DateTimeOffset CreatedAt => User.CreatedAt;

        [JsonIgnore]
        public string Username => User.Username;

        [JsonIgnore]
        public short Discrim => User.Discrim;

        [JsonIgnore]
        public bool IsBot => User.IsBot;

        public string GetAvatarUrl() => User.GetAvatarUrl();

        public override string ToString() =>
            $"{(Nickname == null ? string.Empty : $"{Nickname} ")}({Username}#{Discrim:0000})";
    }
}