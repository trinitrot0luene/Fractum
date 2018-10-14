using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Fractum.Entities;
using Fractum.Entities.WebSocket;
using Fractum.WebSocket.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fractum.WebSocket.EventModels
{
    public sealed class GuildCreateEventModel : BaseEventModel
    {
        internal GuildCreateEventModel()
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

        public override void ApplyToCache(FractumCache cache)
        {
            var gc = new GuildCache(cache.Client, this);
            cache.Guilds.AddOrUpdate(Id, gc, (k, v) => v = gc);
        }

        #region Cacheable Entities

        public List<GuildChannel> Channels;
        public List<Presence> Presences;
        public List<GuildMember> Members;
        public List<Role> Roles;

        [JsonProperty("emojis")]
        public List<GuildEmoji> Emojis { get; private set; }

        #endregion

        #region Properties

        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("icon")]
        public string Icon { get; private set; }

        [JsonProperty("splash")]
        public string Splash { get; private set; }

        [JsonProperty("owner_id")]
        public ulong OwnerId { get; private set; }

        [JsonProperty("region")]
        public string Region { get; private set; }

        [JsonProperty("afk_channel_id")]
        public ulong? AfkChannelId { get; private set; }

        [JsonProperty("afk_timeout")]
        public int AfkTimeout { get; private set; }

        [JsonProperty("embed_enabled")]
        public bool? EmbedEnabled { get; private set; }

        [JsonProperty("embed_channel_id")]
        public ulong? EmbedChannelId { get; private set; }

        [JsonProperty("verification_level")]
        public int VerificationLevel { get; private set; }

        [JsonProperty("default_message_notifications")]
        public int DefaultMessageNotifications { get; private set; }

        [JsonProperty("explicit_content_filter")]
        public int ExplicitContentFilter { get; private set; }

        [JsonProperty("mfa_level")]
        public bool RequireMfa { get; private set; }

        [JsonProperty("unavailable")]
        public bool IsUnavailable { get; private set; }

        [JsonProperty("member_count")]
        public int MemberCount { get; private set; }

        [JsonProperty("lazy")]
        public bool Lazy { get; private set; }

        [JsonProperty("large")]
        public bool Large { get; private set; }

        #endregion
    }
}