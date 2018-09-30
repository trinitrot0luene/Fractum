using System;
using System.Collections.Generic;
using System.Text;

namespace Fractum.WebSocket.Pipelines
{
    /// <summary>
    /// Provides connection-state level caching of payloads.
    /// </summary>
    public interface IStateCache
    {
        string SessionId { get; set; }

        int Seq { get; set; }

        int ReconnectionAttempts { get; set; }

        int HeartbeatInterval { get; set; }

        bool IsWaitingForACK { get; set; }

        void Reset();
    }
}
