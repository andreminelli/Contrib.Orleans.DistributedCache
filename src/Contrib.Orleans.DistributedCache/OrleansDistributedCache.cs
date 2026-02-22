using System;
using System.Threading;
using System.Threading.Tasks;

using Contrib.Orleans.DistributedCache.Grains;

using Microsoft.Extensions.Caching.Distributed;
using Orleans;

namespace Contrib.Orleans.DistributedCache;

/// <summary>
/// Orleans-based implementation of IDistributedCache.
/// </summary>
/// <param name="grainFactory">The Orleans grain factory.</param>
/// <exception cref="ArgumentNullException">Thrown when grainFactory is null.</exception>
public class OrleansDistributedCache(IGrainFactory grainFactory) : IDistributedCache
{
    private readonly IGrainFactory _grainFactory = grainFactory ?? throw new ArgumentNullException(nameof(grainFactory));

    /// <summary>
    /// Gets a value from the cache.
    /// Warning: sync-over-async here!
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <returns>The cached value, or null if not found or expired.</returns>
    /// <exception cref="ArgumentException">Thrown when key is null or empty.</exception>
    public byte[]? Get(string key)
        => GetAsync(key).GetAwaiter().GetResult();

    /// <summary>
    /// Gets a value from the cache asynchronously.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The cached value, or null if not found or expired.</returns>
    /// <exception cref="ArgumentException">Thrown when key is null or empty.</exception>
    public async Task<byte[]?> GetAsync(string key, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var grain = GetGrain(key);
        var result = await grain.GetAsync();

        return result.Exists ? result.Value : null;
    }

    /// <summary>
    /// Sets a value in the cache.
    /// Warning: sync-over-async here!
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="options">The cache entry options.</param>
    /// <exception cref="ArgumentException">Thrown when key is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when value or options is null.</exception>
    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        => SetAsync(key, value, options).GetAwaiter().GetResult();

    /// <summary>
    /// Sets a value in the cache asynchronously.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="options">The cache entry options.</param>
    /// <param name="token">The cancellation token.</param>
    /// <exception cref="ArgumentException">Thrown when key is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when value or options is null.</exception>
    public async Task SetAsync(
        string key,
        byte[] value,
        DistributedCacheEntryOptions options,
        CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(options);

        var grain = GetGrain(key);

        DateTimeOffset? absoluteExpiration = null;
        TimeSpan? slidingExpiration = null;

        if (options.AbsoluteExpiration.HasValue)
        {
            absoluteExpiration = options.AbsoluteExpiration.Value;
        }
        else if (options.AbsoluteExpirationRelativeToNow.HasValue)
        {
            absoluteExpiration = DateTimeOffset.UtcNow.Add(options.AbsoluteExpirationRelativeToNow.Value);
        }

        if (options.SlidingExpiration.HasValue)
        {
            slidingExpiration = options.SlidingExpiration.Value;
        }

        await grain.SetAsync(value, absoluteExpiration, slidingExpiration);
    }

    /// <summary>
    /// Refreshes the sliding expiration of a cached entry.
    /// Warning: sync-over-async here!
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <exception cref="ArgumentException">Thrown when key is null or empty.</exception>
    public void Refresh(string key)
        => RefreshAsync(key).GetAwaiter().GetResult();

    /// <summary>
    /// Refreshes the sliding expiration of a cached entry asynchronously.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="token">The cancellation token.</param>
    /// <exception cref="ArgumentException">Thrown when key is null or empty.</exception>
    public async Task RefreshAsync(string key, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var grain = GetGrain(key);
        await grain.RefreshAsync();
    }

    /// <summary>
    /// Removes a value from the cache.
    /// Warning: sync-over-async here!
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <exception cref="ArgumentException">Thrown when key is null or empty.</exception>
    public void Remove(string key)
        => RemoveAsync(key).GetAwaiter().GetResult();

    /// <summary>
    /// Removes a value from the cache asynchronously.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="token">The cancellation token.</param>
    /// <exception cref="ArgumentException">Thrown when key is null or empty.</exception>
    public async Task RemoveAsync(string key, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var grain = GetGrain(key);
        await grain.RemoveAsync();
    }

    private ICacheGrain<byte[]> GetGrain(string key)
    {
        return _grainFactory.GetGrain<ICacheGrain<byte[]>>(key);
    }

}
