# Orleans.DistributedCache

A NuGet library that implements `IDistributedCache` using Microsoft Orleans as the distributed backend. Each cache key is represented as a separate Orleans grain, enabling seamless integration with Orleans-based applications.

## Features

- ✅ **Full IDistributedCache Implementation** - Implements all sync and async methods
- ✅ **Grain-Based Storage** - Each cache key maps to a separate Orleans grain
- ✅ **Expiration Support** - Supports absolute and sliding expiration
- ✅ **State Persistence** - Optional state management for persistence
- ✅ **Async-First Design** - Built with async/await patterns
- ✅ **.NET 10 & C# 14** - Modern C# features with nullable reference types

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

The `AddOrleansDistributedCacheGrains()` method automatically configures memory grain storage for cache entries.

## Technical Details

### Grain Identity
- Cache keys directly map to Orleans string grain IDs
- Each cache key gets its own `ICacheGrain<byte[]>` grain instance
- Grains are automatically activated on first access via `IGrainFactory.GetGrain<ICacheGrain<byte[]>>(key)`

### State Management
- Uses Orleans `IPersistentState<CacheGrainState<T>>` for state management
- By default, memory grain storage is used (`AddMemoryGrainStorage("cache-storage")`)
- State includes: cached value, absolute expiration, sliding expiration, and last access time
- For persistent storage, configure a different storage provider before calling `AddOrleansDistributedCacheGrains()`

### Expiration Handling
- **On-demand cleanup**: Expired entries are not proactively cleaned up. Instead, expiration is checked on access (Get, Refresh), and expired entries are cleared from state at that time
- Expired entries return `null` to callers and are removed from state

### Thread Safety
- Orleans guarantees single-threaded grain execution
- No additional locking required
- Safe for concurrent access from multiple Orleans silos
- All operations on a specific cache key are serialized through the grain

### Performance
- Grain per key = direct, fast lookups
- Distributed across Orleans cluster
- State persistence is async and non-blocking
- Synchronous methods (`Get`, `Set`, `Remove`, `Refresh`) use `.GetAwaiter().GetResult()` internally
    - This is acceptable for Orleans-based cache implementations where latency is typically high
    - Prefer async methods when possible to avoid blocking threads

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
