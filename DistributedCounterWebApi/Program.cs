using DistributedCounterGrains;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

const string clusterId = "SiloDemoCluster";
const string serviceId = "SiloDemo";

const string connectionString = @"Server=localhost\SQLEXPRESS;Database=Orleans;Trusted_Connection=True;";
const string storageProviderNamespace = "System.Data.SqlClient";

var orleansClient = new ClientBuilder()
    .UseAdoNetClustering(opt =>
    {
        opt.ConnectionString = connectionString;
        opt.Invariant = storageProviderNamespace;
    })
    .Configure<ClusterOptions>(opt =>
    {
        opt.ClusterId = clusterId;
        opt.ServiceId = serviceId;
    })
    .ConfigureApplicationParts(parts =>
        parts.AddApplicationPart(typeof(DistributedCounterGrain<>).Assembly).WithReferences())
    .Build();

await orleansClient.Connect().ConfigureAwait(false);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOrleansDistributedCounter(opt =>
{
    opt.DefaultDelayDeactivation = TimeSpan.FromMinutes(20);
    opt.PersistWhenSet = true;
});
builder.Services.AddSingleton(orleansClient);
builder.Services.Configure<ConsoleLifetimeOptions>(opt => opt.SuppressStatusMessages = true);
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Logging.AddConsole();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/increment/{key}", async (string key, IDistributedCounter distributedCounterService) =>
{
    var distributedCounter = await distributedCounterService.IncrementAsync(key);
    return distributedCounter;
})
.WithName("IncrementDistributedCounter");

app.MapPost("/decrement/{key}", async (string key, IDistributedCounter distributedCounterService) =>
{
    var distributedCounter = await distributedCounterService.DecrementAsync(key);
    return distributedCounter;
}).WithName("DecrementDistributedCounter");

app.Run();