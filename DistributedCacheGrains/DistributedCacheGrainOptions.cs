namespace DistributedCacheGrains;

public class DistributedCacheGrainOptions
{
    public DistributedCacheGrainOptions() { }

    public bool PersistWhenSet { get; set; } = true;

    public TimeSpan DefaultDelayDeactivation { get; set; } = TimeSpan.FromMinutes(5);
}