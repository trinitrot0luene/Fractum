using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class BanAddHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var eventData = (BanAddEventModel) args;

            var guild = cache[eventData.GuildId];
            var member = guild.GetMember(eventData.User.Id);

            client.InvokeLog(new LogMessage(nameof(BanAddHook),
                $"{eventData.User} was banned in {guild.Guild.Name}", LogSeverity.Info));

            client.InvokeMemberBanned(member);

            return Task.CompletedTask;
        }
    }
}