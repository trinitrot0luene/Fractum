using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Fractum.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fractum.WebSocket.Entities
{
    public sealed class GuildCreateModel : DiscordEntity
    {
        internal GuildCreateModel()
        {
            Channels = new List<GuildChannel>();
            Presences = new List<Presence>();
            Members = new List<GuildMember>();
            Roles = new List<Role>();
        }

        [JsonProperty("channels")]
        private JArray RawChannels
        {
            set
            {
                value.ToList().ForEach(token =>
                {
                    GuildChannel newChannel = null;
                    switch (token.Value<int>("type"))
                    {
                        case (int) ChannelType.GuildText:
                            newChannel = token.ToObject<TextChannel>();
                            break;
                        case (int) ChannelType.GuildVoice:
                            newChannel = token.ToObject<VoiceChannel>();
                            break;
                        case (int) ChannelType.GuildCategory:
                            newChannel = token.ToObject<Category>();
                            break;
                    }

                    Channels.Add(newChannel);
                });
            }
        }

        [JsonProperty("presences")]
        private ReadOnlyCollection<Presence> RawPresences
        {
            set
            {
                foreach (var val in value)
                    Presences.Add(val);
            }
        }

        [JsonProperty("members")]
        private ReadOnlyCollection<GuildMember> RawMembers
        {
            set
            {
                foreach (var val in value)
                    Members.Add(val);
            }
        }

        [JsonProperty("roles")]
        private ReadOnlyCollection<Role> RawRoles
        {
            set
            {
                foreach (var val in value)
                    Roles.Add(val);
            }
        }

        #region Cacheable Entities

        public List<GuildChannel> Channels;
        public List<Presence> Presences;
        public List<GuildMember> Members;
        public List<Role> Roles;

        [JsonProperty("emojis")]
        public List<Emoji> Emojis { get; private set; }

        #endregion

        #region Public Properties

        [JsonProperty("owner_id")]
        public ulong OwnerId { get; private set; }

        [JsonProperty("unavailable")]
        public bool IsUnavailable { get; private set; }

        [JsonProperty("region")]
        public string Region { get; private set; }

        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("member_count")]
        public int MemberCount { get; private set; }

        [JsonProperty("lazy")]
        public bool Lazy { get; private set; }

        [JsonProperty("large")]
        public bool Large { get; private set; }

        #endregion
    }
}