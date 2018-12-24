using System.Diagnostics;
using System.Threading.Tasks;
using Fractum;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class GuildCreateHook : IEventHook<EventModelBase>
    {
        public async Task RunAsync(EventModelBase args, FractumCache cache, GatewaySession session)
        {
            var eventModel = (GuildCreateEventModel) args;

            var gc = new SyncedGuildCache(cache, eventModel);

            cache.AddOrReplace(gc);

            if (cache.Client.RestClient.Config.AlwaysDownloadMembers &&
                gc.MemberCount > cache.Client.RestClient.Config.LargeThreshold)
                await cache.Client.RequestMembersAsync(gc.Id);

            cache.Client.InvokeLog(new LogMessage(nameof(GuildCreateHook), $"Guild Available: {gc.Name}", LogSeverity.Info));

            cache.Client.InvokeGuildCreated(gc.Guild);
        }
    }
}