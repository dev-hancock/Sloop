namespace Sloop.Core;

using Abstractions;
using Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
///     Applies any pending schema and table migrations for the Sloop cache.
/// </summary>
public class SloopCacheMigrator : IDbCacheMigrator
{
    private readonly IDbConnectionFactory _factory;

    private readonly ILogger<SloopCacheMigrator> _logger;

    private readonly SloopOptions _options;

    /// <summary>
    ///     Constructs a new instance of <see cref="SloopCacheMigrator" />.
    /// </summary>
    /// <param name="factory">The factory used to create database connections.</param>
    /// <param name="options">The configured <see cref="SloopOptions" />.</param>
    /// <param name="logger">The logger <see cref="ILogger" /></param>
    public SloopCacheMigrator(IDbConnectionFactory factory, IOptions<SloopOptions> options, ILogger<SloopCacheMigrator> logger)
    {
        _factory = factory;
        _logger = logger;
        _options = options.Value;
    }

    /// <summary>
    ///     Executes the cache infrastructure migration: creates the schema, table, and index
    ///     if they do not already exist and <see cref="SloopOptions.CreateInfrastructure" /> is true.
    /// </summary>
    /// <param name="ct">A <see cref="CancellationToken" /> to cancel the migration.</param>
    public async Task MigrateAsync(CancellationToken ct = default)
    {
        if (!_options.CreateInfrastructure)
        {
            _logger.SkippingMigration();

            return;
        }

        _logger.StartingMigration(_options.GetQualifiedTableName());

        await using var connection = await _factory.Create(ct);

        await using var cmd = connection.CreateCommand();

        cmd.CommandText =
            $"""
             CREATE SCHEMA IF NOT EXISTS {_options.GetEffectiveSchema()};

             CREATE UNLOGGED TABLE IF NOT EXISTS {_options.GetQualifiedTableName()} (
                key TEXT NOT NULL PRIMARY KEY,
                value BYTEA NULL,
                expires_at TIMESTAMPTZ NULL,
                sliding_interval INTERVAL NULL,
                absolute_expiry TIMESTAMPTZ NULL
             );

             CREATE INDEX IF NOT EXISTS "{_options.TableName}_expires_at"
             ON {_options.GetQualifiedTableName()} (expires_at);
             """;

        _logger.ExecutingSql(cmd.CommandText);

        await cmd.ExecuteNonQueryAsync(ct);

        _logger.MigrationDone();
    }
}