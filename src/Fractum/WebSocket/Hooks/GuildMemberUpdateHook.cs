using System.Threading.Tasks;
using Fractum;
using Fractum.WebSocket;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class GuildMemberUpdateHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, GatewayCache cache, GatewaySession session)
        {
            var presenceUpdate = (GuildMemberUpdateEventModel) args;

            if (cache.TryGetGuild(presenceUpdate.GuildId ?? 0, out var guild) &&
                guild.TryGet(presenceUpdate.User.Id, out CachedMember member))
            {
                member.Update(presenceUpdate);
                cache.Client.InvokeLog(new LogMessage(nameof(GuildMemberUpdateHook), $"{member.Nickname ?? member.Username + "#" + member.Discrim} was updated.", LogSeverity.Debug));
            }

            return Task.CompletedTask;
        }
    }
}