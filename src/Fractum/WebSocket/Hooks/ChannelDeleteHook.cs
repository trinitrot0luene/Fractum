using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.Entities.WebSocket;
using Fractum.Utilities;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class ChannelDeleteHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, ISession session, FractumSocketClient client)
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

            if (cache.HasGuild(deletedChannel.GuildId))
                cache[deletedChannel.GuildId].Remove(deletedChannel);

            client.InvokeLog(new LogMessage(nameof(ChannelDeleteHook), $"Channel {deletedChannel.Name} was deleted",
                LogSeverity.Verbose));

            client.InvokeChannelDeleted(new CachedEntity<CachedGuildChannel>(deletedChannel));

            return Task.CompletedTask;
        }
    }
}