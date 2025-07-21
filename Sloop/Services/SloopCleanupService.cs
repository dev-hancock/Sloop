namespace Sloop.Services;

using Abstractions;
using Commands;
using Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

/// <summary>
///     A hosted background service that periodically purges expired entries
///     from the PostgreSQL-backed distributed cache.
/// </summary>
internal class SloopCleanupService : BackgroundService
{
    private readonly IDbConnectionFactory _connection;

    private readonly IDbCacheOperations _operations;

    private readonly SloopOptions _options;

    /// <summary>
    ///     Constructs a new instance of the <see cref="SloopCleanupService" />.
    /// </summary>
    /// <param name="options">
    ///     The configured <see cref="SloopOptions" />, including the cleanup interval setting.
    /// </param>
    /// <param name="connection">
    ///     The connection factory used to access PostgreSQL.
    /// </param>
    /// <param name="operations">
    ///     The cache operations used to purge expired items and acquire distributed locks.
    /// </param>
    public SloopCleanupService(IOptions<SloopOptions> options, IDbConnectionFactory connection, IDbCacheOperations operations)
    {
        _options = options.Value;
        _connection = connection;
        _operations = operations;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using var connection = await _connection.Create(stoppingToken);

            if (await _operations.TryAcquireLock.ExecuteAsync(connection, new TryAcquireLockArgs(42_000), stoppingToken))
            {
                await _operations.PurgeExpiredItems.ExecuteAsync(connection, new PurgeExpiredItemsArgs(), stoppingToken);
            }

            await Task.Delay(_options.CleanupInterval, stoppingToken);
        }
    }
}