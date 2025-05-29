namespace Sloop.Benchmarks;

using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

[MemoryDiagnoser]
[ThreadingDiagnoser]
public class SloopBenchmark : IAsyncDisposable
{
    private IDistributedCache _cache = null!;

    private PostgreSqlContainer _db = null!;

    [Params(1, 10, 50)] public int Parallelism;

    public async ValueTask DisposeAsync()
    {
        await _db.DisposeAsync();
    }

    [GlobalSetup]
    public async Task Setup()
    {
        _db = new PostgreSqlBuilder()
            .WithDatabase("db")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        await _db.StartAsync();

        var services = new ServiceCollection();

        services.AddCache(opt =>
        {
            opt.ConnectionString = _db.GetConnectionString();
            opt.SchemaName = "public";
            opt.TableName = "cache";
            opt.CleanupInterval = TimeSpan.FromMinutes(60);
        });

        var provider = services.BuildServiceProvider();

        _cache = provider.GetRequiredService<IDistributedCache>();

        await _cache.SetAsync("bench-key",
            new byte[100],
            new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(1)
            });
    }

    [Benchmark]
    public async Task Sloop_SetAsync()
    {
        await _cache.SetAsync("sloop-" + Guid.NewGuid(),
            new byte[100],
            new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(1)
            });
    }

    [Benchmark]
    public async Task Sloop_GetAsync()
    {
        await _cache.GetAsync("bench-key");
    }

    [Benchmark]
    public async Task Sloop_SetAsync_Parallel()
    {
        var tasks = Enumerable
            .Range(0, Parallelism)
            .Select(_ => _cache.SetAsync(
                Guid.NewGuid().ToString(),
                new byte[100],
                new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(1)
                }));

        await Task.WhenAll(tasks);
    }

    [Benchmark]
    public async Task Sloop_GetAsync_Parallel()
    {
        var tasks = Enumerable
            .Range(0, Parallelism)
            .Select(_ => _cache.GetAsync("bench-key"));

        await Task.WhenAll(tasks);
    }
}