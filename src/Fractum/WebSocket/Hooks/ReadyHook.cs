using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.Entities;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket.Hooks
{
    internal sealed class ReadyHook : IEventHook<EventModelBase>
    {
        public Task RunAsync(EventModelBase args, FractumCache cache, ISession session, FractumSocketClient client)
        {
            var eventModel = (ReadyEventModel) args;

            client.InvokeLog(new LogMessage(nameof(ReadyHook), "Ready", LogSeverity.Info));

            client.InvokeReady();

            return Task.CompletedTask;
        }
    }
}