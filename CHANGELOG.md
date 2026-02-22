# Changelog

All notable changes to Orleans.DistributedCache will be documented in this file.

## [1.0.0] - 2026-02-20

### Added

#### Core Features
- Full implementation of `IDistributedCache` interface
- Grain-based cache storage with Orleans
- Support for absolute expiration
- Support for sliding expiration
- Support for combined absolute + sliding expiration
- Comprehensive input validation

#### Cache Operations
- `Get()` and `GetAsync()` - Retrieve cached values
- `Set()` and `SetAsync()` - Store values with optional expiration
- `Remove()` and `RemoveAsync()` - Delete cached entries
- `Refresh()` and `RefreshAsync()` - Refresh sliding expiration

#### Grain Implementation
- `ICacheGrain<T>` interface for type-safe grain operations
- `CacheGrain<T>` implementation with Orleans state management
- Automatic expiration checking on access
- Thread-safe grain execution via Orleans

#### Dependency Injection
- `ServiceCollectionExtensions.AddOrleansDistributedCache()` - Register cache in DI
- `SiloBuilderExtensions.AddOrleansDistributedCacheGrains()` - Configure grains in silo

#### Testing
- 10+ comprehensive unit tests using xUnit
- AAA pattern (Arrange-Act-Assert) for all tests
- NSubstitute mocking for grain factory
- Shouldly fluent assertions
- Tests for:
  - Basic get/set operations
  - Expiration scenarios
  - Null key validation
  - Sliding expiration refresh
  - Synchronous operations
  - Non-existent key handling

#### Documentation
- Complete README with usage examples
- API reference documentation
- Configuration guides
- Architecture documentation
- NuGet publishing guidelines

#### Code Quality
- .NET 10 & C# 14 compliance
- Nullable reference types enabled
- Code analyzers enabled (latest level)
- Warnings treated as errors
- XML documentation on all public members
- Clean code with proper naming conventions

### Dependencies
- `Microsoft.Orleans.Runtime` (8.7.4)
- `Microsoft.Extensions.Caching.Abstractions` (10.0.0)

### Test Dependencies
- `xunit` (2.8.1)
- `Microsoft.NET.Test.Sdk` (17.13.0)
- `NSubstitute` (5.2.0)
- `Shouldly` (4.2.1)

### Notes

- Initial release with production-ready implementation
- All sync methods use `GetAwaiter().GetResult()` internally
- Grains stay active until silo shutdown or explicit removal
- State persistence is optional and configurable
- No automatic TTL-based cleanup (expired entries removed on access)

## [Unreleased]

### Planned Features
- Configuration options for cache behavior
- Batch operations support
- Cache statistics and metrics
- Memory limit enforcement
- TTL-based cleanup using Orleans reminder timers
