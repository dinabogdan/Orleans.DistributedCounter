using Microsoft.Extensions.Caching.Distributed;
using Orleans;
using Orleans.Concurrency;

namespace DistributedCacheGrains;

public class DistributedCacheService : IDistributedCache
{
    public const string OrleansDistributedCacheStorageProviderName = "OrleansDistributedCacheStorageProvider";

    private const string UseAsyncOnlyMessage = "OrleansDistributedCacheService only supports asynchronous operations";

    private readonly IClusterClient _clusterClient;

    public DistributedCacheService(IClusterClient clusterClient)
    {
        _clusterClient = clusterClient;
    }

    [Obsolete(UseAsyncOnlyMessage)]
    public byte[] Get(string key)
        => SyncOverAsync.Run(() => GetAsync(key));

    public async Task<byte[]> GetAsync(string key, CancellationToken token = new())
        => (await _clusterClient.GetGrain<IDistributedCacheGrain<byte[]>>(key).Get()).Value;

    [Obsolete(UseAsyncOnlyMessage)]
    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        => SyncOverAsync.Run(() => SetAsync(key, value, options));

    public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = new())
    {
        var created = DateTimeOffset.UtcNow;
        var expires = AbsoluteExpiration(created, options);
        var seconds = ExpirationSeconds(created, expires, options);

        return _clusterClient.GetGrain<IDistributedCacheGrain<byte[]>>(key)
            .Set(new Immutable<byte[]>(value), TimeSpan.FromSeconds(seconds ?? 0));
    }

    public void Refresh(string key)
        => SyncOverAsync.Run(() => RefreshAsync(key));

    public Task RefreshAsync(string key, CancellationToken token = new())
        => _clusterClient.GetGrain<IDistributedCacheGrain<byte[]>>(key).Refresh();

    public void Remove(string key)
        => SyncOverAsync.Run(() => RemoveAsync(key));

    public Task RemoveAsync(string key, CancellationToken token = new())
        => _clusterClient.GetGrain<IDistributedCacheGrain<byte[]>>(key).Clear();

    private static long? ExpirationSeconds(DateTimeOffset creationTime,
        DateTimeOffset? absoluteExpiration,
        DistributedCacheEntryOptions options)
    {
        if (absoluteExpiration.HasValue && options.SlidingExpiration.HasValue)
        {
            return (long)Math.Min(
                (absoluteExpiration.Value - creationTime).TotalSeconds,
                options.SlidingExpiration.Value.TotalSeconds);
        }

        if (absoluteExpiration.HasValue)
            return (long)(absoluteExpiration.Value - creationTime).TotalSeconds;

        if (options.SlidingExpiration.HasValue)
            return (long)options.SlidingExpiration.Value.TotalSeconds;

        return null;

    }

    private static DateTimeOffset? AbsoluteExpiration(DateTimeOffset creationTime, DistributedCacheEntryOptions options)
        => options.AbsoluteExpirationRelativeToNow.HasValue ? creationTime + options.AbsoluteExpirationRelativeToNow : options.AbsoluteExpiration;

    private static class SyncOverAsync
    {
        private static readonly TaskFactory Factory = new(CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);

        public static void Run(Func<Task> task) => Factory.StartNew(task).Unwrap().GetAwaiter().GetResult();

        public static TResult Run<TResult>(Func<Task<TResult>> task) => Factory.StartNew(task).Unwrap().GetAwaiter().GetResult();
    }
}