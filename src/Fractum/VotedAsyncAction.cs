using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fractum
{
    /// <summary>
    ///     Initiates an asynchronous periodic action on a target entity, which runs until there are no callers remaining.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class VotedAsyncAction<TEntity>
    {
        private readonly Func<TEntity, Task> _asyncAction;

        private readonly TEntity _entity;

        private readonly int _periodMilliseconds;
        private readonly object _voteLock = new object();

        private Timer _actionTimer;

        private volatile int _actionVotes = 1;

        public VotedAsyncAction(TEntity entity, Func<TEntity, Task> asyncAction, int periodMilliseconds)
        {
            _periodMilliseconds = periodMilliseconds;
            _asyncAction = asyncAction;
            _entity = entity;

            _actionTimer = new Timer(_ => asyncAction(entity), null, 0, periodMilliseconds);
        }

        public void Vote()
        {
            lock (_voteLock)
            {
                if (_actionTimer == null)
                {
                    _actionVotes = 1;
                    _actionTimer = new Timer(_ => _asyncAction(_entity), null, 0, _periodMilliseconds);
                }
                else
                {
                    Interlocked.Increment(ref _actionVotes);
                }
            }
        }

        public void Leave()
        {
            lock (_voteLock)
            {
                if (Interlocked.Decrement(ref _actionVotes) <= 0)
                {
                    _actionTimer.Dispose();
                    _actionTimer = null;
                }
            }
        }
    }
}