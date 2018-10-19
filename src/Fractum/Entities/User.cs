using System;
using Fractum.Contracts;
using Fractum.Entities.WebSocket;
using Newtonsoft.Json;

namespace Fractum.Entities
{
    public class User : DiscordEntity, IUser, ICloneable
    {
        internal User()
        {
        }

        [JsonProperty("avatar")]
        private string AvatarRaw { get; set; }

        [JsonProperty("member", NullValueHandling = NullValueHandling.Ignore)]
        internal PartialMember Member { get; private set; }

        [JsonProperty("username")]
        public string Username { get; internal set; }

        [JsonProperty("discriminator")]
        public short Discrim { get; internal set; }

        [JsonProperty("bot")]
        public bool IsBot { get; private set; }

        [JsonIgnore]
        public string Mention
        {
            get => string.Format(Consts.USER_MENTION, Id);
        }

        public string GetAvatarUrl()
        {
            if (AvatarRaw is null)
                return string.Concat(Consts.CDN, string.Format(Consts.CDN_DEFAULT_AVATAR, Discrim % 5));
            if (AvatarRaw.StartsWith("a_"))
                return string.Concat(Consts.CDN,
                    string.Format(Consts.CDN_USER_AVATAR, Id, AvatarRaw.Substring(2), "gif"));
            return string.Concat(Consts.CDN, string.Format(Consts.CDN_USER_AVATAR, Id, AvatarRaw, ".png"));
        }

        public override string ToString()
            => $"{Id}";

        public object Clone()
        {
            return new User
            {
                Id = this.Id,
                AvatarRaw = this.AvatarRaw,
                Member = this.Member,
                Username = this.Username,
                Discrim = this.Discrim,
                IsBot = this.IsBot
            };
        }
    }
}