using Fractum.WebSocket.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fractum.WebSocket.Pipelines
{
    public sealed class ConnectionStage : IConnectionStage<Payload>
    {
        public IStateCache State { get; private set; }

        public SocketWrapper Socket { get; private set; }

        public ConnectionStage(IStateCache state, SocketWrapper socket)
        {
            State = state;
            Socket = socket;
        }

        public Task CompleteAsync(Payload payload)
        {
            switch (payload.OpCode)
            {
                case OpCode.Hello:
                    return ResumeOrIdentifyAsync();
            }

            return Task.CompletedTask;
        }
    }
}
