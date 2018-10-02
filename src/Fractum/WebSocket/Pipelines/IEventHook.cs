using Fractum.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fractum.WebSocket.Pipelines
{
    public interface IEventHook<TArgs>
    {
        Task RunAsync(TArgs args, IFractumCache cache, ISession session, FractumSocketClient client);
    }
}
