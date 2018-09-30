using System;
using System.Collections.Generic;
using System.Text;

namespace Fractum.WebSocket.Pipelines
{
    public sealed class StateCache : IStateCache
    {
        public string SessionId { get; set; }

        public int Seq { get; set; }

        public int ReconnectionAttempts { get; set; }

        public int HeartbeatInterval { get; set; }

        public bool IsWaitingForACK { get; set; }

        public void Reset()
        {
            SessionId = default;
            Seq = default;
            ReconnectionAttempts = default;
            HeartbeatInterval = default;
            IsWaitingForACK = default;
        }
    }
}
