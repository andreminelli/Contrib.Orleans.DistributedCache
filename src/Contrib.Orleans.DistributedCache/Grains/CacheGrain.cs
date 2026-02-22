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

    /// <inheritdoc/>
    public async Task<CacheResult<T>> GetAsync()
    {
        if (!_state.State.HasValue)
        {
            return CacheResult<T>.NotFound;
        }

        var now = DateTimeOffset.UtcNow;

        if (_state.State.AbsoluteExpiration <= now)
        {
            _state.State.HasValue = false;
            await _state.WriteStateAsync();
            return CacheResult<T>.NotFound;
        }

        if (_state.State.SlidingExpiration.HasValue)
        {
            var lastAccess = _state.State.LastAccessTime ?? now;
            var slidingExpireTime = lastAccess.Add(_state.State.SlidingExpiration.Value);

            if (slidingExpireTime <= now)
            {
                _state.State.HasValue = false;
                await _state.WriteStateAsync();
                return CacheResult<T>.NotFound;
            }

            _state.State.LastAccessTime = now;
            await _state.WriteStateAsync();

            return CacheResult<T>.Found(_state.State.Value, now.Add(_state.State.SlidingExpiration.Value));
        }

        return CacheResult<T>.Found(_state.State.Value, _state.State.AbsoluteExpiration);
    }

    /// <inheritdoc/>
    public async Task SetAsync(T value, DateTimeOffset? absoluteExpiration, TimeSpan? slidingExpiration)
    {
        _state.State.Value = value;
        _state.State.HasValue = true;
        _state.State.AbsoluteExpiration = absoluteExpiration;
        _state.State.SlidingExpiration = slidingExpiration;
        _state.State.LastAccessTime = DateTimeOffset.UtcNow;

        await _state.WriteStateAsync();
    }

    /// <inheritdoc/>
    public async Task RemoveAsync()
    {
        _state.State.HasValue = false;
        _state.State.Value = default;
        _state.State.AbsoluteExpiration = null;
        _state.State.SlidingExpiration = null;
        _state.State.LastAccessTime = null;

        await _state.WriteStateAsync();
    }

    /// <inheritdoc/>
    public async Task RefreshAsync()
    {
        if (!_state.State.HasValue || !_state.State.SlidingExpiration.HasValue)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;

        if (_state.State.AbsoluteExpiration.HasValue && _state.State.AbsoluteExpiration.Value <= now)
        {
            _state.State.HasValue = false;
            await _state.WriteStateAsync();
            return;
        }

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
