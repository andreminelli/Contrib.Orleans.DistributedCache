using System;

namespace Contrib.Orleans.DistributedCache.Grains;

/// <summary>
/// Grain state for storing cache entries with expiration metadata.
/// </summary>
/// <typeparam name="T">The type of data stored in the cache.</typeparam>
public class CacheGrainState<T>
{
    /// <summary>
    /// Gets or sets the cached value.
    /// </summary>
    public T? Value { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the cache entry has a value.
    /// </summary>
    public bool HasValue { get; set; }

    /// <summary>
    /// Gets or sets the absolute expiration time.
    /// </summary>
    public DateTimeOffset? AbsoluteExpiration { get; set; }

    /// <summary>
    /// Gets or sets the sliding expiration duration.
    /// </summary>
    public TimeSpan? SlidingExpiration { get; set; }

    /// <summary>
    /// Gets or sets the last access time for sliding expiration.
    /// </summary>
    public DateTimeOffset? LastAccessTime { get; set; }
}
