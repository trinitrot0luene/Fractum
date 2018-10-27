using System.Collections.Generic;
using Fractum.Entities.WebSocket;

namespace Fractum.Entities
{
    public interface IMessageChannel : ITextChannel
    {
        IEnumerable<CachedMessage> Messages { get; }
    }
}