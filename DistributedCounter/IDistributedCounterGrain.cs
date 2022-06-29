using Orleans;
using Orleans.Concurrency;

namespace DistributedCounterGrains;

public interface IDistributedCounterGrain<T> : IGrainWithStringKey
{
    Task Set(Immutable<T> value, TimeSpan delayDeactivation);

    Task<Immutable<T>> Get();

    Task Refresh();

    Task Clear();
}