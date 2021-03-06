﻿using Fractum.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fractum.Rest
{
    public class RestUser : RestEntity, IUser, ICloneable
    {
        internal RestUser()
        {
        }

        [JsonProperty("avatar")]
        internal string AvatarRaw { get; set; }

        public object Clone() => new RestUser
        {
            Id = Id,
            AvatarRaw = AvatarRaw,
            Username = Username,
            DiscrimValue = DiscrimValue,
            IsBot = IsBot
        };

        [JsonProperty("username")]
        public string Username { get; internal set; }

        [JsonProperty("discriminator")]
        public short DiscrimValue { get; internal set; }

        [JsonProperty("bot")]
        public bool IsBot { get; private set; }

        [JsonIgnore]
        public string Mention => string.Format(Consts.USER_MENTION, Id);

        public string GetAvatarUrl()
        {
            if (AvatarRaw is null)
                return string.Concat(Consts.CDN, string.Format(Consts.CDN_DEFAULT_AVATAR, DiscrimValue % 5));
            if (AvatarRaw.StartsWith("a_"))
                return string.Concat(Consts.CDN,
                    string.Format(Consts.CDN_USER_AVATAR, Id, AvatarRaw.Substring(2), "gif"));
            return string.Concat(Consts.CDN, string.Format(Consts.CDN_USER_AVATAR, Id, AvatarRaw, ".png"));
        }

        public override string ToString()
            => $"{Id}";
    }
}
