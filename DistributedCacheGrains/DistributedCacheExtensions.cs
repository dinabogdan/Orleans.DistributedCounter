using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace DistributedCacheGrains;

public static class DistributedCacheExtensions
{
    public static IServiceCollection AddOrleansDistributedCache(this IServiceCollection services, Action<DistributedCacheGrainOptions> options)
    {
        services.AddOptions().Configure<DistributedCacheGrainOptions>(options);
        services.AddSingleton<IDistributedCache, DistributedCacheService>();
        return services;
    }
}