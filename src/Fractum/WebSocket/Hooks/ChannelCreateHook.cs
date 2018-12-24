using System.Threading.Tasks;
using Fractum;
using Fractum.WebSocket;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class ChannelCreateHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, GatewaySession session)
        {
            var eventModel = (ChannelCreateUpdateOrDeleteEventModel) args;

            CachedChannel createdChannel = null;
            switch (eventModel.Type)
            {
                case ChannelType.GuildCategory:
                    createdChannel = new CachedCategory(cache, eventModel);
                    break;
                case ChannelType.GuildText:
                    createdChannel = new CachedTextChannel(cache, eventModel);
                    break;
                case ChannelType.GuildVoice:
                    createdChannel = new CachedVoiceChannel(cache, eventModel);
                    break;
                case ChannelType.DM:
                    createdChannel = new CachedDMChannel(cache, eventModel);
                    break;
            }

            if (eventModel.Type != ChannelType.DM)
            {
                var guildChannel = createdChannel as CachedGuildChannel;

                if (cache.TryGetGuild(guildChannel.GuildId, out var guild))
                {
                    guild.AddOrReplace(guildChannel);

                    cache.Client.InvokeLog(new LogMessage(nameof(ChannelCreateHook),
                        $"Channel {guildChannel.Name} was created", LogSeverity.Verbose));
                }

                cache.Client.InvokeChannelCreated(guildChannel);
            }
            else
            {
                var dmChannel = createdChannel as CachedDMChannel;

                cache.AddOrReplace(dmChannel);

                cache.Client.InvokeLog(new LogMessage(nameof(ChannelCreateHook), "Private channel was created", LogSeverity.Debug));

                cache.Client.InvokeChannelCreated(dmChannel);
            }

            return Task.CompletedTask;
        }
    }
}