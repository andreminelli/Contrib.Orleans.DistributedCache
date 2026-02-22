using System;
using System.Threading.Tasks;

using Contrib.Orleans.DistributedCache.Grains;

using Orleans.Hosting;
using Orleans.TestingHost;

using Shouldly;

using Xunit;

namespace Contrib.Orleans.DistributedCache.UnitTests;

public class CacheGrainTests : IAsyncLifetime
{
    private InProcessTestCluster? _cluster;

    public async ValueTask InitializeAsync()
    {
        var builder = new InProcessTestClusterBuilder(initialSilosCount: 1);
        builder.ConfigureSilo((options, siloBuilder) => siloBuilder.AddMemoryGrainStorage("cache-storage"));
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
    public async Task CacheGrain_SetGetRemove_Workflow()
    {
        var grain = _cluster!.Client.GetGrain<ICacheGrain<byte[]>>("test-key");

        var (existsBefore, _, _) = await grain.GetAsync();
        existsBefore.ShouldBeFalse();

        var value = new byte[] { 1, 2, 3 };
        await grain.SetAsync(value, DateTimeOffset.UtcNow.AddMinutes(5), TimeSpan.FromMinutes(1));
        var (existsAfter, stored, _) = await grain.GetAsync();
        existsAfter.ShouldBeTrue();
        stored.ShouldNotBeNull();
        stored.ShouldBe(value);

        await grain.RemoveAsync();

        var (existsFinal, _, _) = await grain.GetAsync();
        existsFinal.ShouldBeFalse();
    }
}
