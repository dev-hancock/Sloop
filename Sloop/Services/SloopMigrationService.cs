namespace Sloop.Services;

using Abstractions;
using Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
///     A background service that runs cache schema migrations once when the host starts.
/// </summary>
internal class SloopMigrationService : BackgroundService
{
    private readonly IDbCacheContext _context;

    private readonly ILogger<SloopMigrationService> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SloopMigrationService" /> class.
    /// </summary>
    /// <param name="context">The cache context used to apply database migrations.</param>
    /// <param name="logger">The logger <see cref="ILogger" /></param>
    public SloopMigrationService(IDbCacheContext context, ILogger<SloopMigrationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _context.MigrateAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Do nothing, the service is stopping
        }
        catch (Exception ex)
        {
            _logger.MigrationFailed(ex);

            throw;
        }

        return Task.CompletedTask;
    }
}