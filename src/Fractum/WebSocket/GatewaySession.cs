using System;
using Fractum.Entities;

namespace Fractum.WebSocket
{
    public sealed class GatewaySession
    {
        internal GatewaySession()
        {
        }

        public string SessionId { get; set; }

        public string GatewayUrl { get; set; }

        public int Seq { get; set; }

        public int ReconnectionAttempts { get; set; }

        public bool Reconnecting { get; set; }

        public bool Invalidated { get; set; }

        public bool WaitingForACK { get; set; }

        public void Invalidate()
        {
            SessionId = default;
            Seq = default;
            ReconnectionAttempts = default;
            Reconnecting = default;
            Invalidated = default;
            WaitingForACK = default;
        }
    }
}