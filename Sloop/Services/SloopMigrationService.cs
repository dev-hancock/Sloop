namespace Sloop.Services;

using Abstractions;
using Microsoft.Extensions.Hosting;

/// <summary>
///     A background service that runs cache schema migrations once when the host starts.
/// </summary>
internal class SloopMigrationService : BackgroundService
{
    private readonly IDbCacheContext _context;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SloopMigrationService" /> class.
    /// </summary>
    /// <param name="context">The cache context used to apply database migrations.</param>
    public SloopMigrationService(IDbCacheContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return _context.MigrateAsync(stoppingToken);
    }
}