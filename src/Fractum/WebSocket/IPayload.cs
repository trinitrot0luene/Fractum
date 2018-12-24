using Fractum.WebSocket;
using Fractum.WebSocket.EventModels;

namespace Fractum.WebSocket
{
    public interface IPayload<out T> where T : EventModelBase
    {
        OpCode OpCode { get; }

        T Data { get; }

        int? Seq { get; }

        string Type { get; }
    }
}