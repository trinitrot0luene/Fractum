using System;

namespace Fractum
{
    public sealed class DisposableScope : IDisposable
    {
        private readonly Action _closeScopeAction;

        public DisposableScope(Action closeScopeAction)
        {
            _closeScopeAction = closeScopeAction;
        }

        public void Dispose()
        {
            _closeScopeAction();
        }
    }

    public sealed class DisposableScope<T> : IDisposable
    {
        private readonly Action<T> _closeScopeAction;

        private readonly T _target;

        public DisposableScope(T target, Action<T> closeScopeAction)
        {
            _closeScopeAction = closeScopeAction;
            _target = target;
        }

        public void Dispose()
        {
            _closeScopeAction(_target);
        }
    }
}