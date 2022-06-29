namespace DistributedCounterGrains;

public class DistributedCounterGrainOptions
{

    public DistributedCounterGrainOptions()
    {

    }

    public bool PersistWhenSet { get; set; } = true;

    public TimeSpan DefaultDelayDeactivation { get; set; } = TimeSpan.FromMinutes(5);
}