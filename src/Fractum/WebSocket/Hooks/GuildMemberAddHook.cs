using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.Entities.WebSocket;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class GuildMemberAddHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var eventModel = (GuildMemberAddEventModel) args;

            cache.AddUser(eventModel.User);

            var member = new CachedMember(cache, eventModel, eventModel.GuildId);

            if (eventModel.GuildId.HasValue)
                cache[eventModel.GuildId.Value].Add(member);

            client.InvokeLog(new LogMessage(nameof(GuildMemberAddHook),
                $"{member} joined {member.Guild?.Name}", LogSeverity.Info));

            client.InvokeMemberJoined(member);

            return Task.CompletedTask;
        }
    }
}