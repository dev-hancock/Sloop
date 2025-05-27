using Microsoft.Extensions.Options;
using Npgsql;

namespace Sloop.Commands;

public record RefreshItemArgs(string Key);

public class RefreshItemCommand : IDbCacheCommand<RefreshItemArgs, bool>
{
    private readonly SloopOptions _options;

    public RefreshItemCommand(IOptions<SloopOptions> options)
    {
        _options = options.Value;
    }
    
    public async Task<bool> ExecuteAsync(NpgsqlConnection connection, RefreshItemArgs itemArgs, CancellationToken token = default)
    {
        await using var cmd = connection.CreateCommand();

        cmd.CommandText =
            $"""
             UPDATE "{_options.SchemaName}"."{_options.TableName}"
             SET expires_at = LEAST(now() + sliding_interval, absolute_expiry)
             WHERE key = @key AND sliding_interval IS NOT NULL;
             """;

        cmd.Parameters.AddWithValue("key", itemArgs.Key);

        var count = await cmd.ExecuteNonQueryAsync(token);

        return count > 0;
    }
}