using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Fractum.WebSocket
{
    /// <summary>
    ///     Credit to Quahu (https://gist.github.com/Quahu/3bdfce90f23f7e67f35751ef484d0424) for this implementation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class CircularBuffer<T> : IList<T>
    {
        private readonly List<T> _buffer;

        private readonly object _lockObject = new object();

        public CircularBuffer(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be a positive integer.");

            Capacity = capacity;
            _buffer = new List<T>(capacity + 1);
        }

        public CircularBuffer(IEnumerable<T> collection)
        {
            Capacity = collection.Count();

            if (Capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(collection), "Collection must contain at least one item.");

            _buffer = new List<T>(collection);
        }

        public CircularBuffer(int capacity, IEnumerable<T> collection)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be a positive integer.");

            if (capacity < collection.Count())
                throw new ArgumentOutOfRangeException(nameof(capacity),
                    "Capacity mustn't be smaller than the amount of items in the collection.");

            Capacity = capacity;
            var list = new List<T>(capacity);
            list.AddRange(collection);
            _buffer = list;
        }

        public int Capacity { get; }

        public bool IsReadOnly => false;

        public int Count => _buffer.Count;

        public T this[int index]
        {
            get
            {
                lock (_lockObject) return _buffer[index];
            }
            set
            {
                lock (_lockObject) _buffer[index] = value;
            }
        }

        public void Add(T item)
            => AddInternal(0, item);

        public void Clear()
        {
            lock (_lockObject)
                _buffer.Clear();
        }

        public bool Contains(T item)
        {
            lock (_lockObject)
                return _buffer.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (_lockObject)
                _buffer.CopyTo(array, arrayIndex);
        }

        public int IndexOf(T item)
        {
            lock (_lockObject)
                return _buffer.IndexOf(item);
        }

        public void Insert(int index, T item)
            => AddInternal(index, item);

        public bool Remove(T item)
        {
            lock (_lockObject)
                return _buffer.Remove(item);
        }

        public void RemoveAt(int index)
        {
            lock (_lockObject)
                _buffer.RemoveAt(index);
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (_lockObject)
                return _buffer.ToList().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        private void AddInternal(int index, T item)
        {
            lock (_lockObject)
            {
                _buffer.Insert(index, item);

                if (Count > Capacity)
                    _buffer.RemoveAt(Count - 1);
            }
        }
    }
}