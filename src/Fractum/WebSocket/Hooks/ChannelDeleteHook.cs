using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.Entities.WebSocket;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class ChannelDeleteHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, ISocketCache<ISyncedGuild> cache, ISession session)
        {
            var eventArgs = (ChannelCreateUpdateOrDeleteEventModel) args;

            CachedGuildChannel deletedChannel = null;
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
            }

            if (cache.TryGetGuild(deletedChannel.Guild.Id, out var guild))
            {
                guild.RemoveChannel(deletedChannel.Id);

                cache.Client.InvokeLog(new LogMessage(nameof(ChannelDeleteHook), $"Channel {deletedChannel.Name} was deleted",
                LogSeverity.Verbose));

                cache.Client.InvokeChannelDeleted(new CachedEntity<CachedGuildChannel>(deletedChannel));
            }

            return Task.CompletedTask;
        }
    }
}