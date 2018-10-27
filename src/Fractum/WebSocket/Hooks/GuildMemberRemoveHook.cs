using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.Entities.WebSocket;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class GuildMemberRemoveHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, ISocketCache<ISyncedGuild> cache, ISession session)
        {
            var eventArgs = (GuildMemberRemoveEventModel) args;

            if (cache.TryGetGuild(eventArgs.GuildId, out var guild) &&
                guild.TryGet(eventArgs.User.Id, out CachedMember member))
            {
                cache.Client.InvokeLog(new LogMessage(nameof(GuildMemberRemoveHook),
                $"{member?.ToString() ?? eventArgs.User.ToString()} left {member?.Guild?.Name ?? "Unknown Guild"}",
                LogSeverity.Info));

                cache.Client.InvokeMemberLeft(member as IUser ?? eventArgs.User);

                guild.RemoveMember(member.Id);
            }

            return Task.CompletedTask;
        }
    }
}