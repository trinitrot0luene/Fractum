using System;
using System.Threading.Tasks;
using Fractum.Entities.Properties;
using Newtonsoft.Json;

namespace Fractum.Entities.Rest
{
    public class RestVoiceChannel : RestGuildChannel
    {
        internal RestVoiceChannel()
        {
        }

        [JsonProperty("user_limit")]
        public int UserLimit { get; private set; }

        [JsonProperty("bitrate")]
        public int Bitrate { get; private set; }

        public async Task<RestVoiceChannel> EditAsync(Action<VoiceChannelProperties> editAction)
        {
            var props = new VoiceChannelProperties
            {
                Name = Name,
                ParentId = ParentId,
                PermissionsOverwrites = Overwrites,
                Bitrate = Bitrate,
                Position = Position,
                UserLimit = UserLimit
            };
            editAction(props);

            return await Client.EditChannelAsync(Id, props) as RestVoiceChannel;
        }
    }
}