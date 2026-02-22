using System;
using System.Threading.Tasks;

using Orleans;

namespace Contrib.Orleans.DistributedCache.Grains;

/// <summary>
/// Represents a cache entry grain.
/// </summary>
/// <typeparam name="T">The type of data stored in the cache.</typeparam>
public interface ICacheGrain<T> : IGrainWithStringKey
{
    /// <summary>
    /// Gets the cached value if it exists and has not expired.
    /// </summary>
    /// <returns>A tuple of (exists, value, expirationTime)</returns>
    Task<(bool Exists, T? Value, DateTimeOffset? ExpirationTime)> GetAsync();

    /// <summary>
    /// Sets the cached value with optional expiration.
    /// </summary>
    Task SetAsync(T value, DateTimeOffset? absoluteExpiration, TimeSpan? slidingExpiration);

    /// <summary>
    /// Removes the cached value.
    /// </summary>
    Task RemoveAsync();

    /// <summary>
    /// Refreshes the sliding expiration time without returning the value.
    /// </summary>
    Task RefreshAsync();
}
