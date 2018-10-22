using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.Entities.WebSocket;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class PresenceUpdateHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var eventModel = (PresenceUpdateEventModel) args;

            var newPresence = new CachedPresence(eventModel.User.Id, eventModel.Activity, eventModel.NewStatus.HasValue ? eventModel.NewStatus.Value :
                cache.GetPresenceOrDefault(eventModel.User.Id)?.Status ?? Status.Offline);

            cache.AddPresence(newPresence);

            client.InvokeLog(new LogMessage(nameof(PresenceUpdateHook), "Presence Update", LogSeverity.Debug));

            return Task.CompletedTask;
        }
    }
}