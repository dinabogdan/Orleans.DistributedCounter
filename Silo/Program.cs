using System.Net;
using DistributedCacheGrains;
using DistributedCounterGrains;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

namespace Silo;

public class Program
{
    private const string ClusterId = "SiloDemoCluster";
    private const string ServiceId = "SiloDemo";

    private const string ConnectionString = @"Server=localhost\SQLEXPRESS;Database=Orleans;Trusted_Connection=True;";
    private const string StorageProviderNamespace = "System.Data.SqlClient";

    public static int Main(string[] args)
    {
        int.TryParse(args[0], out var siloPort);
        return RunMainAsync(siloPort).Result;
    }

    private static async Task<int> RunMainAsync(int siloPort)
    {
        try
        {
            var host = await StartSilo(siloPort);
            Console.WriteLine("\n\n Press enter to terminate...\n\n");
            Console.ReadLine();

            await host.StopAsync();

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());

            return 1;
        }
    }

    private static async Task<ISiloHost> StartSilo(int siloPort)
    {
        var builder = new SiloHostBuilder()
            .UseAdoNetClustering(opt =>
            {
                opt.ConnectionString = ConnectionString;
                opt.Invariant = StorageProviderNamespace;
            })
            .AddAdoNetGrainStorage(
                DistributedCounterService.OrleansDistributedCounterStorageProvider,
                opt =>
                {
                    opt.ConnectionString = ConnectionString;
                    opt.Invariant = StorageProviderNamespace;
                })
            .Configure<ClusterOptions>(opt =>
            {
                opt.ClusterId = ClusterId;
                opt.ServiceId = ServiceId;
            })
            .Configure<EndpointOptions>(opt =>
            {
                opt.AdvertisedIPAddress = IPAddress.Loopback;
                opt.SiloPort = siloPort;
                opt.GatewayPort = 30000;
            })
            .ConfigureApplicationParts(parts =>
                parts.AddApplicationPart(typeof(DistributedCacheGrain<>).Assembly).WithReferences())
            .ConfigureApplicationParts(parts =>
                parts.AddApplicationPart(typeof(DistributedCounterGrain<>).Assembly).WithReferences())
            .ConfigureServices(svc =>
                svc.Configure<ConsoleLifetimeOptions>(opt => opt.SuppressStatusMessages = true))
            .ConfigureLogging(logging => logging.AddConsole());

        var host = builder.Build();
        await host.StartAsync();
        return host;
    }
}