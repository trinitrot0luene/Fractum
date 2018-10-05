using Fractum.Entities.Contracts;
using Fractum.WebSocket.Entities;
using Newtonsoft.Json;

namespace Fractum.Entities
{
    public sealed class User : DiscordEntity, IUser
    {
        [JsonProperty("discriminator")]
        private string DiscrimRaw { get; set; }

        [JsonProperty("avatar")]
        private string AvatarRaw { get; set; }

        [JsonProperty("member", NullValueHandling = NullValueHandling.Ignore)]
        internal PartialMember Member { get; private set; }

        [JsonProperty("username")]
        public string Username { get; internal set; }

        [JsonIgnore]
        public short Discrim
        {
            get => short.TryParse(DiscrimRaw ?? string.Empty, out var discrim) ? discrim : short.MinValue;
            internal set => DiscrimRaw = value.ToString();
        }

        [JsonProperty("bot")]
        public bool IsBot { get; private set; }

        public string GetAvatarUrl()
        {
            if (AvatarRaw is null)
                return string.Concat(Consts.CDN, string.Format(Consts.CDN_DEFAULT_AVATAR, Discrim % 5));
            if (AvatarRaw.StartsWith("a_"))
                return string.Concat(Consts.CDN,
                    string.Format(Consts.CDN_USER_AVATAR, Id, AvatarRaw.Substring(2), "gif"));
            return string.Concat(Consts.CDN, string.Format(Consts.CDN_USER_AVATAR, Id, AvatarRaw, ".png"));
        }
    }
}