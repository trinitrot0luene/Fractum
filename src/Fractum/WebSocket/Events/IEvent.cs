using Fractum.WebSocket.Pipelines;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fractum.WebSocket.Events
{
    internal interface IEvent
    {
        void ApplyToCache(FractumCache cache);
    }
}
