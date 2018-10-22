using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class GuildMemberRemoveHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var eventArgs = (GuildMemberRemoveEventModel) args;

            var member = cache[eventArgs.GuildId]?.GetMember(eventArgs.User.Id);

            if (member != null)
                cache[eventArgs.GuildId]?.Remove(member);

            client.InvokeLog(new LogMessage(nameof(GuildMemberRemoveHook),
                $"{member?.ToString() ?? eventArgs.User.ToString()} left {member?.Guild?.Name ?? "Unknown Guild"}",
                LogSeverity.Info));

            client.InvokeMemberLeft(member as IUser ?? eventArgs.User);

            return Task.CompletedTask;
        }
    }
}