using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.Entities.WebSocket;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class GuildMemberAddHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, ISocketCache<ISyncedGuild> cache, ISession session)
        {
            var eventModel = (GuildMemberAddEventModel) args;

            cache.AddOrReplace(eventModel.User);

            var member = new CachedMember(cache, eventModel, eventModel.GuildId);

            if (eventModel.GuildId.HasValue)
            {
                cache.Client.InvokeLog(new LogMessage(nameof(GuildMemberAddHook),
                $"{member} joined {member.Guild?.Name}", LogSeverity.Info));

                cache.Client.InvokeMemberJoined(member);
            }

            return Task.CompletedTask;
        }
    }
}