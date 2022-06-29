using Microsoft.Extensions.DependencyInjection;

namespace DistributedCounterGrains;

public static class DistributedCounterExtensions
{
    public static IServiceCollection AddOrleansDistributedCounter(this IServiceCollection services,
        Action<DistributedCounterGrainOptions> options)
    {
        services.AddOptions().Configure(options);
        services.AddSingleton<IDistributedCounter, DistributedCounterService>();
        return services;
    }
}