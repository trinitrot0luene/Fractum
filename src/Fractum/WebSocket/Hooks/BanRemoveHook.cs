using System.Threading.Tasks;
using Fractum;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class BanRemoveHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, GatewayCache cache, GatewaySession session)
        {
            var eventData = (BanRemoveEventModel) args;

            if (cache.TryGetGuild(eventData.GuildId, out var guild))
            {
                if (!cache.HasUser(eventData.User.Id))
                    cache.AddOrReplace(eventData.User);

                cache.Client.InvokeLog(new LogMessage(nameof(BanRemoveHook),
                $"{eventData.User} was unbanned in {guild?.Guild.Name ?? "Unknown Guild"}", LogSeverity.Info));

                cache.Client.InvokeMemberUnbanned(cache.TryGetUser(eventData.User.Id, out var user) ? user : default);
            }

            return Task.CompletedTask;
        }
    }
}