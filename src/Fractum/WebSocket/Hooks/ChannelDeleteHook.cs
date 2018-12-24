using System.Threading.Tasks;
using Fractum;
using Fractum.WebSocket;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class ChannelDeleteHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, GatewaySession session)
        {
            var eventArgs = (ChannelCreateUpdateOrDeleteEventModel) args;

            CachedChannel deletedChannel = null;
            switch (eventArgs.Type)
            {
                case ChannelType.GuildCategory:
                    deletedChannel = new CachedCategory(cache, eventArgs);
                    break;
                case ChannelType.GuildText:
                    deletedChannel = new CachedTextChannel(cache, eventArgs);
                    break;
                case ChannelType.GuildVoice:
                    deletedChannel = new CachedVoiceChannel(cache, eventArgs);
                    break;
                case ChannelType.DM:
                    deletedChannel = new CachedDMChannel(cache, eventArgs);
                    break;
            }

            if (deletedChannel.Type != ChannelType.DM && deletedChannel is CachedGuildChannel guildChannel &&
                cache.TryGetGuild(guildChannel.Guild.Id, out var guild))
            {
                guild.RemoveChannel(guildChannel.Id);

                cache.Client.InvokeLog(new LogMessage(nameof(ChannelDeleteHook), $"Channel {guildChannel.Name} was deleted",
                LogSeverity.Verbose));

                cache.Client.InvokeChannelDeleted(new Cacheable<CachedGuildChannel>(guildChannel));
            }
            else if (cache.HasDmChannel(deletedChannel.Id))
            {
                cache.RemoveDmChannel(deletedChannel.Id);

                cache.Client.InvokeLog(new LogMessage(nameof(ChannelDeleteHook), "DM Channel was deleted", LogSeverity.Debug));
            }

            return Task.CompletedTask;
        }
    }
}