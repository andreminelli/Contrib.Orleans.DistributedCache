using Microsoft.Extensions.Caching.Distributed;

using NSubstitute;
using Orleans;

namespace Contrib.Orleans.DistributedCache.UnitTests;

public class OrleansDistributedCacheTests : IAsyncLifetime
{
    private IGrainFactory _grainFactory = null!;
    private ICacheGrain<byte[]> _grain = null!;
    private OrleansDistributedCache _cache = null!;

    public ValueTask InitializeAsync()
    {
        _grain = Substitute.For<ICacheGrain<byte[]>>();
        _grainFactory = Substitute.For<IGrainFactory>();
        _grainFactory.GetGrain<ICacheGrain<byte[]>>(Arg.Any<string>()).Returns(_grain);
        _cache = new OrleansDistributedCache(_grainFactory);
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    [Fact]
    public async Task GetAsync_KeyNotFound_ReturnsNull()
    {
        // Arrange
        _grain.GetAsync().Returns(Task.FromResult(CacheResult<byte[]>.NotFound));

        // Act
        var result = await _cache.GetAsync("missing");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetAsync_KeyFound_ReturnsValue()
    {
        // Arrange
        var value = new byte[] { 1, 2, 3 };
        _grain.GetAsync().Returns(Task.FromResult(CacheResult<byte[]>.Found(value, null)));

        // Act
        var result = await _cache.GetAsync("found");

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(value);
    }

    [Fact]
    public void Get_SyncOverAsync_ReturnsValue()
    {
        // Arrange
        var value = new byte[] { 4, 5, 6 };
        _grain.GetAsync().Returns(Task.FromResult(CacheResult<byte[]>.Found(value, null)));

        // Act
        var result = _cache.Get("sync");

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(value);
    }

    [Fact]
    public async Task SetAsync_WithAbsoluteRelativeAndSliding_PassesCorrectArgumentsToGrain()
    {
        // Arrange
        var value = new byte[] { 7 };
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
            SlidingExpiration = TimeSpan.FromMinutes(1)
        };

        // Act
        await _cache.SetAsync("set-key", value, options);

        // Assert: check that grain.SetAsync was invoked with values roughly matching now + 5 minutes
        var tolerance = TimeSpan.FromSeconds(5);
        _grain.Received(1).SetAsync(
            value,
            Arg.Is<DateTimeOffset?>(d => d.HasValue && Math.Abs((d.Value - DateTimeOffset.UtcNow.AddMinutes(5)).TotalSeconds) < tolerance.TotalSeconds),
            Arg.Is<TimeSpan?>(s => s == TimeSpan.FromMinutes(1)));
    }

    [Fact]
    public async Task RefreshAsync_CallsGrainRefresh()
    {
        // Arrange

        // Act
        await _cache.RefreshAsync("refresh");

        // Assert
        _grain.Received(1).RefreshAsync();
    }

    [Fact]
    public async Task RemoveAsync_CallsGrainRemove()
    {
        // Arrange

        // Act
        await _cache.RemoveAsync("remove");

        // Assert
        _grain.Received(1).RemoveAsync();
    }

    [Fact]
    public async Task SetAsync_NullOrInvalidArguments_Throws()
    {
        // Arrange
        var value = new byte[] { 8 };
        var options = new DistributedCacheEntryOptions();

        // Act / Assert
        await Should.ThrowAsync<ArgumentException>(async () => await _cache.SetAsync("", value, options));
        await Should.ThrowAsync<ArgumentNullException>(async () => await _cache.SetAsync("k", null!, options));
        await Should.ThrowAsync<ArgumentNullException>(async () => await _cache.SetAsync("k", value, null!));
    }
}
