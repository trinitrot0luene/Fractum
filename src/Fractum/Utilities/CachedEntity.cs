using System;
using System.Threading.Tasks;
using Fractum.Contracts;
using Fractum.WebSocket.Core;

namespace Fractum.Utilities
{
    /// <summary>
    ///     Wraps an entity that may exist in the <see cref="FractumCache" />, and will otherwise be retrieved asynchronously
    ///     from the API.
    /// </summary>
    /// <typeparam name="TEntity">The entity wrapped by the cache.</typeparam>
    public class CachedEntity<TEntity> where TEntity : IDiscordEntity
    {
        /// <summary>
        ///     The entity, if it exists in the <see cref="FractumCache" />.
        /// </summary>
        private readonly TEntity _entity;

        /// <summary>
        ///     The asynchronous operation required to retrieve the <see cref="TEntity" /> from the API if it doesn't exist.
        /// </summary>
        internal Func<Task<TEntity>> GetFunc;

        /// <summary>
        ///     Create a new <see cref="CachedEntity{TEntity}" />.
        /// </summary>
        /// <param name="entity">The entity, if it exists in the <see cref="FractumCache" />.</param>
        /// <param name="getFunc">The asynchronous operation required to retrieve the entity from the API if it doesn't exist.</param>
        internal CachedEntity(TEntity entity = default, Func<Task<TEntity>> getFunc = default)
        {
            _entity = entity;

            GetFunc = getFunc;
        }

        /// <summary>
        ///     Get the entity from the <see cref="FractumCache" /> or download it from the API if it cannot be found.
        /// </summary>
        /// <returns></returns>
        public Task<TEntity> GetAsync()
            => _entity == null ? GetFunc.Invoke() : Task.FromResult(_entity);

        /// <summary>
        ///     Get the value of the cached item.
        /// </summary>
        /// <returns></returns>
        public TEntity GetValue()
            => _entity;
    }
}