using System;
using System.Runtime.Serialization;

namespace Fractum.WebSocket.Entities
{
    [Serializable]
    public class GatewayException : Exception
    {
        internal GatewayException()
        {
        }

        internal GatewayException(string message) : base(message)
        {
        }

        internal GatewayException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal GatewayException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}