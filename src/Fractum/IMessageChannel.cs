using System.Collections.Generic;
using Fractum.WebSocket;

namespace Fractum
{
    public interface IMessageChannel : ITextChannel
    {
        IEnumerable<CachedMessage> Messages { get; }
    }
}