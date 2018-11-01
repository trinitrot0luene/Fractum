using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.Entities.WebSocket;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class BanAddHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, GatewaySession session)
        {
            var eventData = (BanAddEventModel) args;

            if (cache.TryGetGuild(eventData.GuildId, out var guild))
            {
                guild.TryGet(eventData.User.Id, out CachedMember member);

                cache.Client.InvokeLog(new LogMessage(nameof(BanAddHook),
                    $"{eventData.User} was banned in {guild.Guild.Name}", LogSeverity.Info));

                cache.Client.InvokeMemberBanned(member);
            }

            return Task.CompletedTask;
        }
    }
}