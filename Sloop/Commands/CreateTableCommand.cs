namespace Sloop.Commands;

using Interfaces;
using Microsoft.Extensions.Options;
using Npgsql;

/// <summary>
///     Represents the input for the <see cref="CreateTableCommand" />.
/// </summary>
public record CreateTableArgs;

/// <summary>
///     Command to ensure the PostgreSQL cache schema and table exist.
///     Executes DDL to create the schema/table if they are not present.
/// </summary>
public class CreateTableCommand : IDbCacheCommand<CreateTableArgs, bool>
{
    private readonly SloopOptions _options;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CreateTableCommand" /> class.
    /// </summary>
    /// <param name="options">The configured Sloop options.</param>
    public CreateTableCommand(IOptions<SloopOptions> options)
    {
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<bool> ExecuteAsync(NpgsqlConnection connection, CreateTableArgs _, CancellationToken token = default)
    {
        await using var cmd = connection.CreateCommand();

        cmd.CommandText =
            $"""
             CREATE SCHEMA IF NOT EXISTS "{_options.SchemaName}";

             CREATE UNLOGGED TABLE IF NOT EXISTS "{_options.SchemaName}"."{_options.TableName}" (
                key TEXT NOT NULL PRIMARY KEY,
                value BYTEA NOT NULL,
                expires_at TIMESTAMPTZ NULL,
                sliding_interval INTERVAL NULL,
                absolute_expiry TIMESTAMPTZ NULL
             );

             CREATE INDEX IF NOT EXISTS "{_options.TableName}_expires_at"
             ON "{_options.SchemaName}"."{_options.TableName}" (expires_at);
             """;

        await cmd.ExecuteNonQueryAsync(token);

        return true;
    }
}