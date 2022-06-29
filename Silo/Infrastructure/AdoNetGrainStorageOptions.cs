using Orleans;
using Orleans.AdoNet;

namespace Silo.Infrastructure;

public class AdoNetGrainStorageOptions
{
    public string ConnectionString { get; set; }

    public string Invariant { get; set; } = DefaultAdoNetInvariant;

    public int InitStage { get; set; } = DefaultInitStage;

    public const string DefaultAdoNetInvariant = AdoNetInvariants.InvariantNameSqlServer;
    public const int DefaultInitStage = ServiceLifecycleStage.ApplicationServices;
}