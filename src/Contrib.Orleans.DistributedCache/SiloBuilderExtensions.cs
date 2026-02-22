using Orleans.Hosting;

namespace Contrib.Orleans.DistributedCache;

/// <summary>
/// Extension methods for configuring cache grains with Orleans silos.
/// </summary>
public static class SiloBuilderExtensions
{
    /// <summary>
    /// Configures cache grains for the Orleans silo.
    /// </summary>
    /// <returns>The silo builder for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when siloBuilder is null.</exception>
    public static ISiloBuilder AddOrleansDistributedCacheGrains(
        this ISiloBuilder siloBuilder)
    {
        ArgumentNullException.ThrowIfNull(siloBuilder);

        siloBuilder.AddMemoryGrainStorage("cache-storage");

        return siloBuilder;
    }
}
