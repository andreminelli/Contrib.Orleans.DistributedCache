using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Contrib.Orleans.DistributedCache;
using Contrib.Orleans.DistributedCache.Grains;

using Orleans.Hosting;
using Orleans.TestingHost;
using Shouldly;
using Xunit;

namespace Contrib.Orleans.DistributedCache.UnitTests;

public class CacheGrainExpirationTests : IAsyncLifetime
{
    private InProcessTestCluster? _cluster;

    public async ValueTask InitializeAsync()
    {
        var builder = new InProcessTestClusterBuilder(initialSilosCount: 1);
        builder.ConfigureSilo((options, siloBuilder) =>
        {
            siloBuilder.AddMemoryGrainStorage("cache-storage");
        });
        _cluster = builder.Build();
        await _cluster.DeployAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_cluster is not null)
        {
            await _cluster.StopAllSilosAsync();
            _cluster.Dispose();
        }
    }

    [Fact]
    public async Task AbsoluteExpiration_EntryExpiresAfterAbsoluteTime()
    {
        // Arrange
        var grain = _cluster!.Client.GetGrain<ICacheGrain<byte[]>>("abs-key");
        var value = new byte[] { 9 };

        // Act
        await grain.SetAsync(value, DateTimeOffset.UtcNow.AddMilliseconds(200), null);

        // Assert immediate exists
        var resultBefore = await grain.GetAsync();
        resultBefore.Exists.ShouldBeTrue();

        // Wait past expiration
        await Task.Delay(400, TestContext.Current.CancellationToken);

        var resultAfter = await grain.GetAsync();
        resultAfter.Exists.ShouldBeFalse();
    }

    [Fact]
    public async Task SlidingExpiration_EntryExpiresWhenNotAccessed()
    {
        // Arrange
        var grain = _cluster!.Client.GetGrain<ICacheGrain<byte[]>>("sliding-expire-key");
        var value = new byte[] { 1 };

        // Act: set with sliding expiration of 200ms
        await grain.SetAsync(value, null, TimeSpan.FromMilliseconds(200));

        // Wait longer than sliding expiration without accessing
        await Task.Delay(350, TestContext.Current.CancellationToken);

        var resultAfter = await grain.GetAsync();
        resultAfter.Exists.ShouldBeFalse();
    }

    [Fact]
    public async Task SlidingExpiration_GetResetsSlidingWindow()
    {
        // Arrange
        var grain = _cluster!.Client.GetGrain<ICacheGrain<byte[]>>("sliding-refresh-key");
        var value = new byte[] { 2 };

        // Act: set with sliding expiration of 300ms
        await grain.SetAsync(value, null, TimeSpan.FromMilliseconds(300));

        // Wait half of sliding then access to reset
        await Task.Delay(150, TestContext.Current.CancellationToken);
        var result1 = await grain.GetAsync();
        result1.Exists.ShouldBeTrue();

        // Wait slightly longer than half again (total > original sliding if not refreshed)
        await Task.Delay(200, TestContext.Current.CancellationToken);

        // Access again, should still exist because previous GetAsync refreshed last access
        var result2 = await grain.GetAsync();
        result2.Exists.ShouldBeTrue();
    }

    [Fact]
    public async Task RefreshAsync_ExtendsSlidingExpirationWithoutReturningValue()
    {
        // Arrange
        var grain = _cluster!.Client.GetGrain<ICacheGrain<byte[]>>("refresh-key");
        var value = new byte[] { 3 };

        // Act: set with sliding expiration of 300ms
        await grain.SetAsync(value, null, TimeSpan.FromMilliseconds(300));

        // Wait half of sliding
        await Task.Delay(150, TestContext.Current.CancellationToken);

        // Refresh to extend sliding expiration
        await grain.RefreshAsync();

        // Wait longer than remaining time before refresh would have expired
        await Task.Delay(200, TestContext.Current.CancellationToken);

        var result = await grain.GetAsync();
        result.Exists.ShouldBeTrue();
    }
}
