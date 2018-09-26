using Fractum.Entities.Contracts;
using Fractum.WebSocket.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fractum.Entities
{
    public sealed class User : DiscordEntity, IUser
    {
        [JsonProperty("username")]
        public string Username { get; internal set; }

        [JsonProperty("discriminator")]
        private string DiscrimRaw { get; set; }

        [JsonIgnore]
        public short Discrim { get => short.TryParse(DiscrimRaw ?? string.Empty, out var discrim) ? discrim : short.MinValue; internal set => DiscrimRaw = value.ToString(); }

        [JsonProperty("avatar")]
        private string AvatarRaw { get; set; }

        [JsonProperty("bot")]
        public bool IsBot { get; private set; }

        [JsonProperty("member", NullValueHandling = NullValueHandling.Ignore)]
        internal PartialMember Member { get; private set; }

        public string GetAvatarUrl()
        {
            if (AvatarRaw is null)
                return string.Concat(Consts.CDN, string.Format(Consts.CDN_DEFAULT_AVATAR, Discrim % 5));
            else if (AvatarRaw.StartsWith("a_"))
                return string.Concat(Consts.CDN, string.Format(Consts.CDN_USER_AVATAR, Id, AvatarRaw.Substring(2), "gif"));
            else
                return string.Concat(Consts.CDN, string.Format(Consts.CDN_USER_AVATAR, Id, AvatarRaw, ".png"));
        }
    }
}
