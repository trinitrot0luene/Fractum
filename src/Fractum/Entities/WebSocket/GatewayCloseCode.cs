using System.Net.WebSockets;

namespace Fractum.Entities.WebSocket
{
    internal static class GatewayCloseCode
    {
        /// <summary>
        ///     4000
        /// </summary>
        public static WebSocketCloseStatus UnknownError => (WebSocketCloseStatus) 4000;

        /// <summary>
        ///     4001
        /// </summary>
        public static WebSocketCloseStatus UnknownOpCode => (WebSocketCloseStatus) 4001;

        /// <summary>
        ///     4002
        /// </summary>
        public static WebSocketCloseStatus DecodeError => (WebSocketCloseStatus) 4002;

        /// <summary>
        ///     4003
        /// </summary>
        public static WebSocketCloseStatus NotAuthenticated => (WebSocketCloseStatus) 4003;

        /// <summary>
        ///     4004
        /// </summary>
        public static WebSocketCloseStatus AuthenticationFailed => (WebSocketCloseStatus) 4004;

        /// <summary>
        ///     4005
        /// </summary>
        public static WebSocketCloseStatus AlreadyAuthenticated => (WebSocketCloseStatus) 4005;

        /// <summary>
        ///     4007
        /// </summary>
        public static WebSocketCloseStatus InvalidSeq => (WebSocketCloseStatus) 4007;

        /// <summary>
        ///     4008
        /// </summary>
        public static WebSocketCloseStatus Ratelimited => (WebSocketCloseStatus) 4008;

        /// <summary>
        ///     4009
        /// </summary>
        public static WebSocketCloseStatus SessionTimedOut => (WebSocketCloseStatus) 4009;

        /// <summary>
        ///     4010
        /// </summary>
        public static WebSocketCloseStatus InvalidShard => (WebSocketCloseStatus) 4010;

        /// <summary>
        ///     4011
        /// </summary>
        public static WebSocketCloseStatus ShardingRequired => (WebSocketCloseStatus) 4011;

        public static string GetCodeName(int closeCode)
        {
            switch (closeCode)
            {
                case 1000:
                    return nameof(WebSocketCloseStatus.NormalClosure);
                case 1001:
                    return nameof(WebSocketCloseStatus.EndpointUnavailable);
                case 1002:
                    return nameof(WebSocketCloseStatus.ProtocolError);
                case 1005:
                    return nameof(WebSocketCloseStatus.Empty);
                case 1008:
                    return nameof(WebSocketCloseStatus.PolicyViolation);
                case 1009:
                    return nameof(WebSocketCloseStatus.MessageTooBig);
                case 1010:
                    return nameof(WebSocketCloseStatus.MandatoryExtension);
                case 1011:
                    return nameof(WebSocketCloseStatus.InternalServerError);
                case 1013:
                    return nameof(WebSocketCloseStatus.InvalidMessageType);
                case 1017:
                    return nameof(WebSocketCloseStatus.InvalidPayloadData);
                case 4000:
                    return nameof(UnknownError);
                case 4001:
                    return nameof(UnknownOpCode);
                case 4002:
                    return nameof(DecodeError);
                case 4003:
                    return nameof(NotAuthenticated);
                case 4004:
                    return nameof(AuthenticationFailed);
                case 4005:
                    return nameof(AlreadyAuthenticated);
                case 4007:
                    return nameof(InvalidSeq);
                case 4008:
                    return nameof(Ratelimited);
                case 4009:
                    return nameof(SessionTimedOut);
                case 4010:
                    return nameof(InvalidShard);
                case 4011:
                    return nameof(ShardingRequired);
                default:
                    return "UnexpectedError";
            }
        }
    }
}