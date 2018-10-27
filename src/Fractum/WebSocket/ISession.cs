using System;

namespace Fractum.WebSocket
{
    /// <summary>
    ///     Provides connection-session level caching of payloads.
    /// </summary>
    public interface ISession
    {
        /// <summary>
        ///     The duration of the current session.
        /// </summary>
        DateTimeOffset Duration { get; set; }

        /// <summary>
        ///     The Id of the session started with the gateway, for resuming.
        /// </summary>
        string SessionId { get; set; }

        /// <summary>
        ///     The URL of the gateway, for reconnecting.
        /// </summary>
        string GatewayUrl { get; set; }

        /// <summary>
        ///     The last sequence received from a gateway event.
        /// </summary>
        int Seq { get; set; }

        /// <summary>
        ///     The amount of times a reconnection attempt has been made using the cached URL.
        /// </summary>
        int ReconnectionAttempts { get; set; }

        /// <summary>
        ///     Gets whether the socket is currently reconnecting.
        /// </summary>
        bool Reconnecting { get; set; }

        /// <summary>
        ///     Whether the session was invalidated or not by an op: 9 INVALIDATE_SESSION or indicated by a close code.
        /// </summary>
        bool Invalidated { get; set; }

        /// <summary>
        ///     Whether we are waiting for an op: 11 HEARTBEAT_ACK.
        /// </summary>
        bool WaitingForACK { get; set; }

        /// <summary>
        ///     Clear all stored session data.
        /// </summary>
        void Invalidate();
    }
}