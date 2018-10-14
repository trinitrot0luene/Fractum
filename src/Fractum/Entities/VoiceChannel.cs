using System;
using System.Threading.Tasks;
using Fractum.Entities.Properties;
using Newtonsoft.Json;

namespace Fractum.Entities
{
    public sealed class VoiceChannel : GuildChannel
    {
        internal VoiceChannel()
        {
        }

        [JsonProperty("user_limit")]
        public int UserLimit { get; private set; }

        [JsonProperty("bitrate")]
        public int Bitrate { get; private set; }

        public async Task<VoiceChannel> EditAsync(Action<VoiceChannelProperties> editAction)
        {
            var props = new VoiceChannelProperties()
            {
                Name = this.Name,
                ParentId = this.ParentId,
                PermissionsOverwrites = this.Overwrites,
                Bitrate = this.Bitrate,
                Position = this.Position,
                UserLimit = this.UserLimit
            };
            editAction(props);

            return await Client.EditChannelAsync(Id, props) as VoiceChannel;
        }
    }
}