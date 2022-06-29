namespace DistributedCounterGrains;

public interface IDistributedCounter
{
    public Task<DistributedCounter> IncrementAsync(string key);

    public Task<DistributedCounter> DecrementAsync(string key);
}