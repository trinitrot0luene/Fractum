using System;
using System.Threading.Tasks;
using Fractum;

namespace Fractum.WebSocket
{
    /// <summary>
    ///     Wraps an entity that may exist in the <see cref="FractumCache" />, and will otherwise be retrieved asynchronously
    ///     from the API.
    /// </summary>
    /// <typeparam name="TEntity">The entity wrapped by the cache.</typeparam>
    public class Cacheable<TEntity> where TEntity : IDiscordEntity
    {
        /// <summary>
        ///     The entity wrapped by the <see cref="Cacheable{TEntity}"/> />.
        /// </summary>
        private readonly TEntity _entity;

        /// <summary>
        ///     The asynchronous operation required to retrieve the <see cref="TEntity" /> if it doesn't exist.
        /// </summary>
        private readonly Func<Task<TEntity>> GetFunc;

        /// <summary>
        ///     Create a new <see cref="Cacheable{TEntity}" />.
        /// </summary>
        /// <param name="entity">The entity to be wrapped.</param>
        /// <param name="getFunc">The asynchronous operation required to retrieve the entity if it doesn't exist.</param>
        internal Cacheable(TEntity entity = default, Func<Task<TEntity>> getFunc = default)
        {
            _entity = entity;

            GetFunc = getFunc;
        }

        /// <summary>
        ///     Get the entity or invoke the retrieval delegate if one is set.
        /// </summary>
        /// <returns></returns>
        public Task<TEntity> GetAsync()
            => _entity != default ? Task.FromResult(_entity) : (GetFunc != default ? GetFunc() : default);

        /// <summary>
        /// Gets the value of the cached entity.
        /// </summary>
        public TEntity Value
            => _entity;

        /// <summary>
        ///     Gets whether the entity has a value.
        /// </summary>
        public bool HasValue => _entity != default;
    }
}