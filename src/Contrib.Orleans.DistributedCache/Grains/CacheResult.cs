using Orleans;

namespace Contrib.Orleans.DistributedCache.Grains;

/// <summary>
/// Represents the result of a cache get operation.
/// </summary>
/// <typeparam name="T">The type of data stored in the cache.</typeparam>
/// <param name="Exists">Indicates whether the cached value exists and has not expired.</param>
/// <param name="Value">The cached value, or default if the cache entry does not exist.</param>
/// <param name="ExpirationTime">The expiration time of the cached value, or null if not applicable.</param>
[GenerateSerializer]
public record CacheResult<T>(bool Exists, T? Value, DateTimeOffset? ExpirationTime)
{
    /// <summary>
    /// Default instance when key is not found
    /// </summary>
    public static CacheResult<T> NotFound => new(false, default, null);

    /// <summary>
    /// Create an instance when key is found
    /// </summary>
    public static CacheResult<T> Found(T? value, DateTimeOffset? expirationTime) => new(true, value, expirationTime);
}
