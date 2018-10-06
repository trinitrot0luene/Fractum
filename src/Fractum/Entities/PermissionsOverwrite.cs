using Newtonsoft.Json;

namespace Fractum.Entities
{
    public class PermissionsOverwrite
    {
        [JsonProperty("id")]
        private string IdRaw { get; set; }

        [JsonIgnore]
        public ulong Id => ulong.Parse(IdRaw);

        [JsonProperty("type")]
        private string TypeRaw { get; set; }

        [JsonProperty("allow")]
        public Permissions Allow { get; private set; }

        [JsonProperty("deny")]
        public Permissions Deny { get; private set; }

        [JsonIgnore]
        public OverwriteType Type => TypeRaw.ToLower() == "role" ? OverwriteType.Role :
            TypeRaw.ToLower() == "member" ? OverwriteType.Member : OverwriteType.Member;
    }

    public enum OverwriteType
    {
        Member,
        Role
    }
}