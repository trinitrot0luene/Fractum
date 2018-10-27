using System;
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
        internal string AvatarRaw { get; set; }

        [JsonProperty("member", NullValueHandling = NullValueHandling.Ignore)]
        internal PartialMember Member { get; private set; }

        public object Clone() => new User
        {
            Id = Id,
            AvatarRaw = AvatarRaw,
            Member = Member,
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