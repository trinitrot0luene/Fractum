using System;

namespace Fractum.Utilities
{
    internal sealed class DisposableScope : IDisposable
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
}