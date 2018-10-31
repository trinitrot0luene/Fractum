using System;
using System.Collections.ObjectModel;
using Fractum.Entities;
using Newtonsoft.Json;

namespace Fractum.WebSocket.EventModels
{
    public class ChannelCreateUpdateOrDeleteEventModel : EventModelBase
    {
        [JsonProperty("id")]
        public ulong Id { get; private set; }

        [JsonProperty("type")]
        public ChannelType Type { get; private set; }

        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("position")]
        public int Position { get; private set; }

        [JsonProperty("guild_id")]
        internal ulong GuildId { get; private set; }

        [JsonProperty("parent_id")]
        internal ulong? ParentId { get; set; }

        [JsonProperty("nsfw")]
        public bool IsNsfw { get; private set; }

        [JsonProperty("topic")]
        public string Topic { get; private set; }

        [JsonProperty("last_pin_timestamp")]
        public DateTimeOffset? LastPinAt { get; private set; }

        [JsonProperty("rate_limit_per_user")]
        public int PerUserRatelimit { get; private set; }

        [JsonProperty("user_limit")]
        public int UserLimit { get; private set; }

        [JsonProperty("bitrate")]
        public int Bitrate { get; private set; }

        [JsonProperty("last_message_id")]
        public ulong? LastMessageId { get; internal set; }

        [JsonProperty("permission_overwrites")]
        public ReadOnlyCollection<PermissionsOverwrite> Overwrites { get; private set; }

        [JsonProperty("recipients")]
        public ReadOnlyCollection<User> Recipients { get; private set; }
    }
}