using Sloop.Commands;

namespace Sloop;

using Microsoft.Extensions.Hosting;

/// <summary>
/// A hosted background service that periodically purges expired entries
/// from the PostgreSQL-backed distributed cache.
/// </summary>
internal class SloopCleanupService : BackgroundService
{
    private readonly IDbConnectionFactory _connection;

    /// <summary>
    /// The interval between successive cleanup runs.
    /// </summary>
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

    private readonly IDbCacheOperations _operations;

    /// <summary>
    /// Constructs a new instance of the <see cref="SloopCleanupService" />.
    /// </summary>
    public SloopCleanupService(IDbConnectionFactory connection, IDbCacheOperations operations)
    {
        _connection = connection;
        _operations = operations;
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
            await using var connection = await _connection.Create(stoppingToken);

            if (await _operations.TryAcquireLock.ExecuteAsync(connection, new TryAcquireLockArgs(42_000), stoppingToken))
            {
                await _operations.PurgeExpiredItems.ExecuteAsync(connection, new PurgeExpiredItemsArgs(), stoppingToken);
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}