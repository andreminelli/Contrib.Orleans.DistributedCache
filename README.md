# Orleans.DistributedCache

A NuGet library that implements `IDistributedCache` using Microsoft Orleans as the distributed backend. Each cache key is represented as a separate Orleans grain, enabling seamless integration with Orleans-based applications.

## Features

- ✅ **Full IDistributedCache Implementation** - Implements all sync and async methods
- ✅ **Grain-Based Storage** - Each cache key maps to a separate Orleans grain
- ✅ **Expiration Support** - Supports absolute and sliding expiration
- ✅ **State Persistence** - Optional state management for persistence
- ✅ **Async-First Design** - Built with async/await patterns
- ✅ **.NET 10 & C# 14** - Modern C# features with nullable reference types
- ✅ **Production Ready** - Comprehensive test coverage with xUnit

## Architecture

### Core Components

**ICacheGrain<T>**
- Grain interface for cache entries
- Supports Get, Set, Remove, and Refresh operations
- Handles expiration logic (absolute and sliding)

**CacheGrain<T>**
- Orleans grain implementation
- Uses Orleans state management for persistence
- Single-threaded execution guarantees
- Automatic expiration checking on access

**OrleansDistributedCache**
- Implements `IDistributedCache` interface
- Creates grain instances for each cache key
- Maps `DistributedCacheEntryOptions` to grain state
- Handles both sync and async operations

## Installation

```bash
dotnet add package Contrib.Orleans.DistributedCache
```

## Usage

### Basic Setup

1. **Configure Orleans Silo**

```csharp
var host = new HostBuilder()
    .UseOrleans(builder =>
    {
        builder
            .UseLocalhostClustering()
            .AddOrleansDistributedCacheGrains();
    })
    .ConfigureServices(services =>
    {
        services.AddOrleansDistributedCache();
    })
    .Build();

await host.RunAsync();
```

2. **Use in Your Application**

```csharp
public class MyService
{
    private readonly IDistributedCache _cache;

    public MyService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task CacheDataAsync(string key, byte[] value)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
            SlidingExpiration = TimeSpan.FromMinutes(5)
        };

        await _cache.SetAsync(key, value, options);
    }

    public async Task<byte[]?> RetrieveDataAsync(string key)
    {
        return await _cache.GetAsync(key);
    }

    public async Task InvalidateDataAsync(string key)
    {
        await _cache.RemoveAsync(key);
    }
}
```

## Configuration

### Absolute Expiration

```csharp
var options = new DistributedCacheEntryOptions
{
    AbsoluteExpiration = DateTimeOffset.UtcNow.AddHours(1)
};

await cache.SetAsync("key", data, options);
```

### Sliding Expiration

```csharp
var options = new DistributedCacheEntryOptions
{
    SlidingExpiration = TimeSpan.FromMinutes(30)
};

await cache.SetAsync("key", data, options);
```

### Combined Expiration

```csharp
var options = new DistributedCacheEntryOptions
{
    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
    SlidingExpiration = TimeSpan.FromMinutes(30)
};

await cache.SetAsync("key", data, options);
```

## API Reference

### IDistributedCache Methods

#### Get/GetAsync
Retrieves a value from the cache. Returns `null` if the key doesn't exist or has expired.

```csharp
byte[]? value = cache.Get("key");
byte[]? value = await cache.GetAsync("key");
```

#### Set/SetAsync
Stores a value in the cache with optional expiration settings.

```csharp
cache.Set("key", data, options);
await cache.SetAsync("key", data, options);
```

#### Remove/RemoveAsync
Removes a value from the cache.

```csharp
cache.Remove("key");
await cache.RemoveAsync("key");
```

#### Refresh/RefreshAsync
Refreshes the sliding expiration time for a cached entry.

```csharp
cache.Refresh("key");
await cache.RefreshAsync("key");
```

## Technical Details

### Grain Identity
- Cache keys directly map to grain string IDs
- Each cache key gets its own grain instance
- Grains are automatically activated on first access

### State Persistence
- Uses Orleans `IPersistentState<T>` for state management
- State is persisted to configured storage provider
- Optional state management for in-memory operation

### Expiration Handling
- Absolute expiration: Entry expires at specific time
- Sliding expiration: Resets on each access
- Combined: Uses whichever expires first
- Expired entries checked on Get, returning null

### Thread Safety
- Orleans guarantees single-threaded grain execution
- No additional locking required
- Safe for concurrent access from multiple silos

### Performance
- Grain per key = direct, fast lookups
- Distributed across Orleans cluster
- State persistence is async and non-blocking

## Testing

The library includes comprehensive tests using:
- **xUnit** - Test framework
- **NSubstitute** - Mocking library
- **Shouldly** - Assertion library

Run tests with:
```bash
dotnet test
```

## Limitations

- Requires Orleans cluster (not suitable for single-process caching)
- Cache values limited by Orleans state storage limits
- No TTL-based cleanup; expired entries removed on access
- Grains stay active until explicitly removed or silo shuts down

## Requirements

- .NET 10.0 or higher
- Microsoft.Orleans.Runtime 8.7.4 or compatible
- Microsoft.Extensions.Caching.Abstractions 10.0.0

## License

MIT

## Contributing

Contributions are welcome! Please ensure:
- Code follows C# 14 standards
- All tests pass
- New features include test coverage
- Documentation is updated

## Support

For issues, questions, or suggestions, please use the GitHub issue tracker.
