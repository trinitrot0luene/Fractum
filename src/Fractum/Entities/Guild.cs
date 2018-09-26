using Fractum.WebSocket.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Fractum.Entities
{
    public sealed class Guild : DiscordEntity
    {
        internal Guild()
        {
            MemberData = new ConcurrentDictionary<ulong, GuildMember>();
            ChannelData = new ConcurrentDictionary<ulong, GuildChannel>();
            RoleData = new ConcurrentDictionary<ulong, Role>();
            PresenceData = new ConcurrentDictionary<ulong, Presence>();
        }

        #region Private Properties

        [JsonProperty("owner_id")]
        private string OwnerIdRaw { get; set; }

        [JsonProperty("channels")]
        private JArray ChannelsRaw
        {
            set
            {
                value.ToList().ForEach(token =>
                {
                    GuildChannel newChannel = null;
                    switch (token.Value<int>("type"))
                    {
                        case (int)ChannelType.GuildText:
                            newChannel = token.ToObject<TextChannel>();
                            break;
                        case (int)ChannelType.GuildVoice:
                            newChannel = token.ToObject<VoiceChannel>();
                            break;
                        case (int)ChannelType.GuildCategory:
                            newChannel = token.ToObject<Category>();
                            break;
                    }
                    ChannelData.AddOrUpdate(newChannel.Id, newChannel, (k, v) => 
                    {
                        newChannel.WithClient(Client);
                        return newChannel ?? v;
                    });
                });
            }
        }

        [JsonProperty("presences")]
        internal ReadOnlyCollection<Presence> Presences
        {
            set
            {
                value.ToList().ForEach(presence =>
                {
                    PresenceData.AddOrUpdate(presence.User.Id, presence, (k, v) => v = presence ?? v);
                });
            }
        }

        #endregion

        #region Public Properties

        [JsonProperty("unavailable")]
        public bool IsUnavailable { get; private set; }

        [JsonProperty("region")]
        public string Region { get; private set; }

        [JsonIgnore]
        public ulong OwnerId { get => ulong.Parse(OwnerIdRaw); }

        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("member_count")]
        public int MemberCount { get; private set; }

        [JsonProperty("lazy")]
        public bool Lazy { get; private set; }

        [JsonProperty("large")]
        public bool Large { get; private set; }

        [JsonProperty("emojis")]
        public Emoji[] Emojis { get; private set; }

        [JsonIgnore]
        public ReadOnlyCollection<TextChannel> TextChannels
        {
            get
            {
                var channelList = ChannelData
                    .Where(c => c.Value.Type == ChannelType.GuildText)
                    .Select(kvp => kvp.Value)
                    .Cast<TextChannel>()
                    .ToList();
                channelList.ForEach(channel =>
                {
                    channel.WithClient(Client);
                    channel.Guild = this;
                });
                return channelList.AsReadOnly();
            }
        }

        [JsonIgnore]
        public ReadOnlyCollection<VoiceChannel> VoiceChannels
        {
            get
            {
                var channelList = ChannelData
                    .Where(c => c.Value.Type == ChannelType.GuildVoice)
                    .Select(kvp => kvp.Value)
                    .Cast<VoiceChannel>()
                    .ToList();
                channelList.ForEach(channel =>
                {
                    channel.WithClient(Client);
                    channel.Guild = this;
                });
                return channelList.AsReadOnly();
            }
        }

        [JsonIgnore]
        public ReadOnlyCollection<Category> Categories
        {
            get
            {
                var channelList = ChannelData
                    .Where(c => c.Value.Type == ChannelType.GuildCategory)
                    .Select(kvp => kvp.Value)
                    .Cast<Category>()
                    .ToList();
                channelList.ForEach(channel =>
                {
                    channel.WithClient(Client);
                    channel.Guild = this;
                });
                return channelList.AsReadOnly();
            }
        }

        [JsonIgnore]
        public ReadOnlyCollection<GuildChannel> Channels
        {
            get
            {
                var channelList = ChannelData.Select(kvp => kvp.Value).ToList();
                channelList.ForEach(channel =>
                {
                    channel.WithClient(Client);
                    channel.Guild = this;
                });
                return channelList.AsReadOnly();
            }
        }

        [JsonProperty("members")]
        public ReadOnlyCollection<GuildMember> Members
        {
            get
            {
                var members = MemberData.Select(kvp => kvp.Value).ToList();
                members.ForEach(member =>
                {
                    member.Roles = RoleData
                        .Where(kvp => member.RoleIds
                        .Any(r => r == kvp.Key))
                        .Select(kvp => kvp.Value)
                        .ToList()
                        .AsReadOnly();
                    member.WithClient(Client);
                    member.Guild = this;
                    if (PresenceData.TryGetValue(member.Id, out var presence))
                        member.Presence = presence;
                });
                return members.AsReadOnly();
            }
            set
            {
                value.ToList().ForEach(m =>
                {
                    MemberData.AddOrUpdate(m.Id, m, (k, v) => v = m ?? v);
                });
            }
        }

        [JsonProperty("roles")]
        public ReadOnlyCollection<Role> Roles
        {
            get
            {
                var roleList = RoleData.Select(kvp => kvp.Value).ToList();
                roleList.ForEach(role => role.WithClient(Client));
                return roleList.AsReadOnly();
            }
            private set
            {
                value.ToList().ForEach(role =>
                {
                    role.WithClient(Client);
                    RoleData.AddOrUpdate(role.Id, role, (k, v) => v = role ?? v);
                });
            }
        }

        #endregion

        #region Entity Caches

        /// <summary>
        /// Caches channels by id, to allow easy modification and fast population from key lookups.
        /// </summary>
        [JsonIgnore]
        internal ConcurrentDictionary<ulong, GuildChannel> ChannelData { get; set; }
        
        /// <summary>
        /// Caches members by id, to allow easy modification and fast population from key lookups.
        /// </summary>
        [JsonIgnore]
        internal ConcurrentDictionary<ulong, GuildMember> MemberData { get; set; }

        /// <summary>
        /// Caches roles by id, to allow easy modification and fast population from key lookups.
        /// </summary>
        [JsonIgnore]
        internal ConcurrentDictionary<ulong, Role> RoleData { get; set; }

        /// <summary>
        /// Caches initial presence data by id, to allow fast population from key lookups.
        /// </summary>
        [JsonIgnore]
        internal ConcurrentDictionary<ulong, Presence> PresenceData { get; set; }

        #endregion
    }
}
