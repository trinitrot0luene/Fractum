using System;
using System.Threading.Tasks;
using Fractum.Entities.Rest;
using Fractum.WebSocket;
using Fractum.WebSocket.EventModels;

namespace Fractum.Entities.WebSocket
{
    public sealed class CachedVoiceChannel : CachedGuildChannel
    {
        internal CachedVoiceChannel(ISocketCache<ISyncedGuild> cache, ChannelCreateUpdateOrDeleteEventModel model,
            ulong? guildId = null) : base(cache, model, guildId)
        {
            UserLimit = model.UserLimit;
            Bitrate = model.Bitrate;
        }

        public int UserLimit { get; private set; }

        public int Bitrate { get; private set; }

        internal new void Update(ChannelCreateUpdateOrDeleteEventModel model)
        {
            base.Update(model);

            UserLimit = model.UserLimit;
            Bitrate = model.Bitrate;
        }

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

            return await Client.RestClient.EditChannelAsync(Id, props) as RestVoiceChannel;
        }
    }
}