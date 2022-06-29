using DistributedCacheGrains;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

namespace SessionStateWebApp;

public class Program
{
    private const string ClusterId = "SiloDemoCluster";
    private const string ServiceId = "SiloDemo#2";

    private const string ConnectionString = @"Server=localhost\SQLEXPRESS;Database=Orleans;Trusted_Connection=True;";
    private const string StorageProviderNamespace = "System.Data.SqlClient";

    public static async Task Main(string[] args)
    {
        IClusterClient orleansClient = new ClientBuilder()
            .UseAdoNetClustering(opt =>
            {
                opt.ConnectionString = ConnectionString;
                opt.Invariant = StorageProviderNamespace;
            })
            .Configure<ClusterOptions>(opt =>
            {
                opt.ClusterId = ClusterId;
                opt.ServiceId = ServiceId;
            })
            .ConfigureApplicationParts(parts =>
                parts.AddApplicationPart(typeof(DistributedCacheGrain<>).Assembly).WithReferences())
            .Build();

        await orleansClient.Connect().ConfigureAwait(false);

        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages();
        builder.Services.AddOrleansDistributedCache(opt =>
        {
            opt.DefaultDelayDeactivation = TimeSpan.FromMinutes(20);
            opt.PersistWhenSet = true;
        });
        builder.Services.AddSession(opt =>
        {
            opt.IdleTimeout = TimeSpan.FromMinutes(20);
            opt.Cookie.IsEssential = true;
        });
        builder.Services.AddSingleton(orleansClient);
        builder.Services.Configure<ConsoleLifetimeOptions>(opt => opt.SuppressStatusMessages = true);
        builder.Logging.AddConsole();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseSession();

        app.UseRouting();

        app.UseAuthorization();

        app.MapRazorPages();

        await app.RunAsync();
    }
}