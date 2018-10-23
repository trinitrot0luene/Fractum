using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.Entities.WebSocket;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class GuildMembersChunkHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var eventModel = (GuildMembersChunkEventModel) args;

            if (cache.HasGuild(eventModel.GuildId))
            {
                var gc = cache[eventModel.GuildId];
                foreach (var rawMember in eventModel.Members)
                {
                    var member = new CachedMember(cache, rawMember, gc.Id);
                    gc.Add(member);
                }
            }

            client.InvokeLog(new LogMessage(nameof(GuildMembersChunkHook),
                $"Received a {eventModel.Members.Count} member chunk for guild {cache[eventModel.GuildId]?.Name}",
                LogSeverity.Debug));

            return Task.CompletedTask;
        }
    }
}