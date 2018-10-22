using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class GuildCreateHook : IEventHook<EventModelBase>
    {
        public async Task RunAsync(EventModelBase args, FractumCache cache, ISession session,
            FractumSocketClient client)
        {
            var eventModel = (GuildCreateEventModel) args;

            var gc = new SyncedGuildCache(cache, eventModel);

            cache.AddOrUpdate(gc, old => old = gc);

            if (client.RestClient.Config.AlwaysDownloadMembers &&
                gc.MemberCount > client.RestClient.Config.LargeThreshold)
                await client.RequestMembersAsync(gc.Id);

            client.InvokeLog(
                new LogMessage(nameof(GuildCreateHook), $"Guild Available: {gc.Name}", LogSeverity.Info));

            client.InvokeGuildCreated(cache[gc.Id].Guild);
        }
    }
}