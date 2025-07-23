namespace Sloop.Services;

using Abstractions;
using Commands;
using Core;
using Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
///     A hosted background service that periodically purges expired entries
///     from the PostgreSQL-backed distributed cache.
/// </summary>
internal class SloopCleanupService : BackgroundService
{
    private const int LockId = 42_000;

    private readonly IDbConnectionFactory _connection;

    private readonly ILogger<SloopCleanupService> _logger;

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
    /// <param name="logger">The logger <see cref="ILogger" /></param>
    public SloopCleanupService(IOptions<SloopOptions> options, IDbConnectionFactory connection, IDbCacheOperations operations,
        ILogger<SloopCleanupService> logger)
    {
        _options = options.Value;
        _connection = connection;
        _operations = operations;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var connection = await _connection.Create(stoppingToken);

                _logger.TryLockStart(LockId);

                var result = await _operations.TryAcquireLock.ExecuteAsync(connection, new TryAcquireLockArgs(LockId), stoppingToken);

                if (result)
                {
                    _logger.TryLockAcquired(LockId);

                    await _operations.PurgeExpiredItems.ExecuteAsync(connection, new PurgeExpiredItemsArgs(), stoppingToken);
                }
                else
                {
                    _logger.TryLockDenied(LockId);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.CleanupFailed(ex);
            }

            try
            {
                await Task.Delay(_options.CleanupInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Do nothing, the service is stopping
            }
        }
    }
}