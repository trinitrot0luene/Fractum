using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class GuildMemberUpdateHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var presenceUpdate = (GuildMemberUpdateEventModel) args;

            var member = cache[presenceUpdate.GuildId ?? 0]?.GetMember(presenceUpdate.User.Id);

            member?.Update(presenceUpdate);

            client.InvokeLog(new LogMessage(nameof(GuildMemberUpdateHook), "Presence Update", LogSeverity.Debug));

            return Task.CompletedTask;
        }
    }
}