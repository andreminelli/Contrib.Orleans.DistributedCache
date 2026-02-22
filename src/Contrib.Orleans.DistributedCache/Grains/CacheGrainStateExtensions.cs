using System;
using System.Threading.Tasks;
using Orleans.Runtime;

namespace Contrib.Orleans.DistributedCache.Grains;

/// <summary>
/// Extension methods for <see cref="IPersistentState{CacheGrainState}"/> to encapsulate state access patterns.
/// </summary>
internal static class CacheGrainStateExtensions
{
    extension<T>(IPersistentState<CacheGrainState<T>> state)
    {
        /// <summary>
        /// Checks if the cache entry has a value.
        /// </summary>
        public bool HasValue()
            => state.State.HasValue;

        /// <summary>
        /// Checks if the cache entry has expired based on absolute expiration.
        /// </summary>
        public bool IsAbsoluteExpired(DateTimeOffset now)
            => state.State.AbsoluteExpiration <= now;

        /// <summary>
        /// Checks if the cache entry has expired based on sliding expiration.
        /// </summary>
        public bool IsSlidingExpired(DateTimeOffset now)
        {
            if (!state.State.SlidingExpiration.HasValue)
            {
                return false;
            }

            var lastAccess = state.State.LastAccessTime ?? now;
            var slidingExpireTime = lastAccess.Add(state.State.SlidingExpiration.Value);

            return slidingExpireTime <= now;
        }

        /// <summary>
        /// Gets the value from the cache state.
        /// </summary>
        public T? GetValue()
            => state.State.Value;

        /// <summary>
        /// Gets the next expiration time for the cache entry.
        /// </summary>
        public DateTimeOffset? GetNextExpiration(DateTimeOffset now)
            => state.State.SlidingExpiration.HasValue ? now.Add(state.State.SlidingExpiration.Value) : state.State.AbsoluteExpiration;

        /// <summary>
        /// Sets the cache value and expiration parameters.
        /// </summary>
        public async Task SetValueAsync(
            T? value,
            DateTimeOffset? absoluteExpiration,
            TimeSpan? slidingExpiration)
        {
            state.State.Value = value;
            state.State.HasValue = true;
            state.State.AbsoluteExpiration = absoluteExpiration;
            state.State.SlidingExpiration = slidingExpiration;
            state.State.LastAccessTime = DateTimeOffset.UtcNow;

            await state.WriteStateAsync();
        }

        /// <summary>
        /// Clears the cache entry.
        /// </summary>
        public async Task ClearAsync()
        {
            state.State.HasValue = false;
            state.State.Value = default;
            state.State.AbsoluteExpiration = null;
            state.State.SlidingExpiration = null;
            state.State.LastAccessTime = null;

            await state.WriteStateAsync();
        }

        /// <summary>
        /// Updates the last access time.
        /// </summary>
        public async Task UpdateLastAccessTimeAsync(DateTimeOffset now)
        {
            state.State.LastAccessTime = now;
            await state.WriteStateAsync();
        }
    }
}

