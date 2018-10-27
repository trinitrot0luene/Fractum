using System.Collections.Generic;

namespace Fractum.WebSocket
{
    public interface IKeyedEnumerable<TKey, TValue> : IEnumerable<TValue>
    {
        TValue this[TKey key] { get; }

        bool TryGetValue(TKey key, out TValue value);
    }
}