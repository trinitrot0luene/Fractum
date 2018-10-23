using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Fractum.Entities;
using Fractum.Entities.Rest;
using Fractum.Entities.WebSocket;
using Fractum.Utilities;

namespace Fractum.Contracts
{
    public interface IMessageChannel : ITextChannel
    {
        IEnumerable<CachedMessage> Messages { get; }
    }
}