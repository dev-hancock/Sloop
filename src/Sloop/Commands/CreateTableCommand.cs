using Microsoft.Extensions.Options;
using Npgsql;

namespace Sloop.Commands;

public record CreateTableArgs;

public class CreateTableCommand : IDbCacheCommand<CreateTableArgs, bool>
{
    private readonly SloopOptions _options;

    private CreateTableCommand(IOptions<SloopOptions> options)
    {
        _options = options.Value;
    }

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