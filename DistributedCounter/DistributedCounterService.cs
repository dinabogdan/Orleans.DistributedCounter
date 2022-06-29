using Orleans;
using Orleans.Concurrency;

namespace DistributedCounterGrains;

public class DistributedCounterService : IDistributedCounter
{
    public const string OrleansDistributedCounterStorageProvider = "OrleansDistributedCounterStorageProvider";

    private readonly IClusterClient _clusterClient;

    public DistributedCounterService(IClusterClient clusterClient)
    {
        _clusterClient = clusterClient;
    }

    public async Task<DistributedCounter> IncrementAsync(string key)
    {
        var counterValue = (await _clusterClient.GetGrain<IDistributedCounterGrain<DistributedCounter>>(key).Get()).Value;
        var incrementedCounterValue = new DistributedCounter
        {
            Value = counterValue != null ? counterValue.Value + 1 : 1,
        };

        await _clusterClient.GetGrain<IDistributedCounterGrain<DistributedCounter>>(key)
            .Set(new Immutable<DistributedCounter>(incrementedCounterValue), TimeSpan.FromSeconds(0));

        return incrementedCounterValue;
    }

    public async Task<DistributedCounter> DecrementAsync(string key)
    {
        var counterValue = (await _clusterClient.GetGrain<IDistributedCounterGrain<DistributedCounter>>(key).Get()).Value;
        var decrementedCounterValue = new DistributedCounter
        {
            Value = counterValue != null ? counterValue.Value - 1 : -1,
        };

        await _clusterClient.GetGrain<IDistributedCounterGrain<DistributedCounter>>(key)
            .Set(new Immutable<DistributedCounter>(decrementedCounterValue), TimeSpan.FromSeconds(0));

        return decrementedCounterValue;
    }
}