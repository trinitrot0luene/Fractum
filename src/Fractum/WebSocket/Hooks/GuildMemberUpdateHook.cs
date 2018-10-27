using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.Entities.WebSocket;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class GuildMemberUpdateHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, ISocketCache<ISyncedGuild> cache, ISession session)
        {
            var presenceUpdate = (GuildMemberUpdateEventModel) args;

            if (cache.TryGetGuild(presenceUpdate.GuildId ?? 0, out var guild) &&
                guild.TryGet(presenceUpdate.User.Id, out CachedMember member))
            {
                member.Update(presenceUpdate);
                cache.Client.InvokeLog(new LogMessage(nameof(GuildMemberUpdateHook), "Presence Update", LogSeverity.Debug));
            }

            return Task.CompletedTask;
        }
    }
}