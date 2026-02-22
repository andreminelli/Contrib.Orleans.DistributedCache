using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;

namespace Contrib.Orleans.DistributedCache.Grains;

/// <summary>
/// Orleans grain implementation for a single cache entry.
/// </summary>
/// <typeparam name="T">The type of data stored in the cache.</typeparam>
/// <remarks>
/// Initializes a new instance of the CacheGrain class.
/// </remarks>
/// <param name="state">The persistent state for this grain.</param>
public class CacheGrain<T>(
    [PersistentState(stateName: "cache", storageName: "cache-storage")]
        IPersistentState<CacheGrainState<T>> state) : Grain, ICacheGrain<T>
{
    private readonly IPersistentState<CacheGrainState<T>> _state = state;

    /// <summary>
    /// Gets the cached value if it exists and has not expired.
    /// </summary>
    /// <returns>A tuple of (exists, value, expirationTime)</returns>
    public async Task<(bool Exists, T? Value, DateTimeOffset? ExpirationTime)> GetAsync()
    {
        if (!_state.State.HasValue)
        {
            return (false, default, null);
        }

        var now = DateTimeOffset.UtcNow;

        // Check absolute expiration if present
        if (_state.State.AbsoluteExpiration.HasValue && _state.State.AbsoluteExpiration.Value <= now)
        {
            _state.State.HasValue = false;
            await _state.WriteStateAsync();
            return (false, default, null);
        }

        // Check sliding expiration if present
        if (_state.State.SlidingExpiration.HasValue)
        {
            var lastAccess = _state.State.LastAccessTime ?? now;
            var slidingExpireTime = lastAccess.Add(_state.State.SlidingExpiration.Value);

            if (slidingExpireTime <= now)
            {
                _state.State.HasValue = false;
                await _state.WriteStateAsync();
                return (false, default, null);
            }

            // Refresh last access time and persist
            _state.State.LastAccessTime = now;
            await _state.WriteStateAsync();

            return (true, _state.State.Value, now.Add(_state.State.SlidingExpiration.Value));
        }

        // No sliding expiration; return with absolute expiration (may be null)
        return (true, _state.State.Value, _state.State.AbsoluteExpiration);
    }

    /// <summary>
    /// Sets the cached value with optional expiration.
    /// </summary>
    /// <param name="value">The value to cache.</param>
    /// <param name="absoluteExpiration">The absolute expiration time.</param>
    /// <param name="slidingExpiration">The sliding expiration duration.</param>
    /// <returns>A task that completes when the value is set.</returns>
    public Task SetAsync(T value, DateTimeOffset? absoluteExpiration, TimeSpan? slidingExpiration)
    {
        _state.State.Value = value;
        _state.State.HasValue = true;
        _state.State.AbsoluteExpiration = absoluteExpiration;
        _state.State.SlidingExpiration = slidingExpiration;
        _state.State.LastAccessTime = DateTimeOffset.UtcNow;

        return _state.WriteStateAsync();
    }

    /// <summary>
    /// Removes the cached value.
    /// </summary>
    /// <returns>A task that completes when the value is removed.</returns>
    public Task RemoveAsync()
    {
        _state.State.HasValue = false;
        _state.State.Value = default;
        _state.State.AbsoluteExpiration = null;
        _state.State.SlidingExpiration = null;
        _state.State.LastAccessTime = null;

        return _state.WriteStateAsync();
    }

    /// <summary>
    /// Refreshes the sliding expiration time without returning the value.
    /// </summary>
    /// <returns>A task that completes when the refresh is done.</returns>
    public async Task RefreshAsync()
    {
        if (!_state.State.HasValue || !_state.State.SlidingExpiration.HasValue)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;

        // Check absolute expiration if present
        if (_state.State.AbsoluteExpiration.HasValue && _state.State.AbsoluteExpiration.Value <= now)
        {
            _state.State.HasValue = false;
            await _state.WriteStateAsync();
            return;
        }

        // Check sliding expiration based on last access
        var lastAccess = _state.State.LastAccessTime ?? now;
        var slidingExpireTime = lastAccess.Add(_state.State.SlidingExpiration.Value);

        if (slidingExpireTime <= now)
        {
            _state.State.HasValue = false;
            await _state.WriteStateAsync();
            return;
        }

        _state.State.LastAccessTime = now;
        await _state.WriteStateAsync();
    }
}
