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
    IPersistentState<CacheGrainState<T>> state)
    : Grain, ICacheGrain<T>
{
    private readonly IPersistentState<CacheGrainState<T>> _state = state;

    /// <inheritdoc/>
    public async Task<CacheResult<T>> GetAsync()
    {
        if (!_state.HasValue())
        {
            return CacheResult<T>.NotFound;
        }

        var now = DateTimeOffset.UtcNow;

        if (_state.IsAbsoluteExpired(now) || _state.IsSlidingExpired(now))
        {
            await _state.ClearAsync();
            return CacheResult<T>.NotFound;
        }

        await _state.UpdateLastAccessTimeAsync(now);

        return CacheResult<T>.Found(_state.GetValue(), _state.GetNextExpiration(now));
    }

    /// <inheritdoc/>
    public async Task SetAsync(T value, DateTimeOffset? absoluteExpiration, TimeSpan? slidingExpiration)
        => await _state.SetValueAsync(value, absoluteExpiration, slidingExpiration);

    /// <inheritdoc/>
    public async Task RemoveAsync()
        => await _state.ClearAsync();

    /// <inheritdoc/>
    public async Task RefreshAsync()
    {
        if (!_state.State.HasValue || !_state.State.SlidingExpiration.HasValue)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;

        if (_state.IsAbsoluteExpired(now) || _state.IsSlidingExpired(now))
        {
            await _state.ClearAsync();
            return;
        }

        await _state.UpdateLastAccessTimeAsync(now);
    }
}
