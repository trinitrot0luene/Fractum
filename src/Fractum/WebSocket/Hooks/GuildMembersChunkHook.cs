using System.Diagnostics;
using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.Entities.WebSocket;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class GuildMembersChunkHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, ISession session)
        {
            var sw = Stopwatch.StartNew();
            var eventModel = (GuildMembersChunkEventModel) args;

            if (cache.TryGetGuild(eventModel.GuildId, out var guild))
            {
                foreach (var rawMember in eventModel.Members)
                {
                    var member = new CachedMember(cache, rawMember, guild.Id);
                    guild.AddOrReplace(member);
                    cache.AddOrReplace(rawMember.User);
                }

                cache.Client.InvokeLog(new LogMessage(nameof(GuildMembersChunkHook),
                $"Received a {eventModel.Members.Count} member chunk for guild {guild.Name}",
                LogSeverity.Debug));
            }

            return Task.CompletedTask;
        }
    }
}