namespace Sloop;

using Microsoft.Extensions.Hosting;

/// <summary>
/// A hosted background service that periodically purges expired entries
/// from the PostgreSQL-backed distributed cache.
/// </summary>
internal class SloopCleanupService : BackgroundService
{
    /// <summary>
    /// The interval between successive cleanup runs.
    /// </summary>
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

    private readonly SloopServices _services;

    /// <summary>
    /// Constructs a new instance of the <see cref="SloopCleanupService" />.
    /// </summary>
    public SloopCleanupService(SloopServices services)
    {
        _services = services;
    }

    /// <summary>
    /// Runs the background cleanup loop. Every <see cref="_interval" />, it attempts to
    /// acquire a distributed advisory lock to ensure only one instance performs cleanup,
    /// then purges expired cache entries in batches.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using var connection = _services.Connection.Create();

            if (await _services.Operations.TryAcquireLock(connection, 42_000, stoppingToken))
            {
                await _services.Operations.PurgeExpired(connection, stoppingToken);
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}