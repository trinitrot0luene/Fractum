using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class BanRemoveHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, ISocketCache<ISyncedGuild> cache, ISession session)
        {
            var eventData = (BanRemoveEventModel) args;

            if (cache.TryGetGuild(eventData.GuildId, out var guild))
            {
                cache.Client.InvokeLog(new LogMessage(nameof(BanRemoveHook),
                $"{eventData.User} was unbanned in {guild?.Guild.Name ?? "Unknown Guild"}", LogSeverity.Info));

                cache.Client.InvokeMemberUnbanned(eventData.User);
            }

            return Task.CompletedTask;
        }
    }
}