using Orleans;
using Orleans.Concurrency;

namespace DistributedCacheGrains;

public interface IDistributedCacheGrain<T> : IGrainWithStringKey
{
    Task Set(Immutable<T> value, TimeSpan delayDeactivation);

    Task<Immutable<T>> Get();

    Task Refresh();

    Task Clear();
}