using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Concurrency;
using Orleans.Providers;

namespace DistributedCacheGrains;

[StorageProvider(ProviderName = DistributedCacheService.OrleansDistributedCacheStorageProviderName)]
public class DistributedCacheGrain<T> : Grain<Immutable<T>>, IDistributedCacheGrain<T>
{
    private TimeSpan _delayDeactivation = TimeSpan.Zero;

    private readonly DistributedCacheGrainOptions _options;

    public DistributedCacheGrain(IOptions<DistributedCacheGrainOptions> options)
    {
        _options = options.Value;
    }

    public async Task Set(Immutable<T> value, TimeSpan delayDeactivation)
    {
        State = value;
        _delayDeactivation = (delayDeactivation > TimeSpan.Zero) ? delayDeactivation : _options.DefaultDelayDeactivation;

        if (_options.PersistWhenSet)
            await base.WriteStateAsync();

        DelayDeactivation(delayDeactivation);
    }

    public Task<Immutable<T>> Get() => Task.FromResult(State);

    public async Task Refresh()
    {
        await base.ReadStateAsync();
        DelayDeactivation(_delayDeactivation);
    }

    public async Task Clear()
    {
        await base.ClearStateAsync();
        DeactivateOnIdle();
    }
}