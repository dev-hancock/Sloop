using Microsoft.Extensions.Options;
using Npgsql;

namespace Sloop.Commands;

public record RemoveItemArgs(string Key);

public class RemoveItemCommand : IDbCacheCommand<RemoveItemArgs, bool>
{
    private readonly SloopOptions _options;

    public RemoveItemCommand(IOptions<SloopOptions> options)
    {
        _options = options.Value;
    }
    
    public async Task<bool> ExecuteAsync(NpgsqlConnection connection, RemoveItemArgs args, CancellationToken token = default)
    {
        await using var cmd = connection.CreateCommand();

        cmd.CommandText =
            $"""
             DELETE FROM "{_options.SchemaName}"."{_options.TableName}"
             WHERE key = @key;
             """;

        cmd.Parameters.AddWithValue("key", args.Key);
        
        var count = await cmd.ExecuteNonQueryAsync(token);

        return count > 0;
    }
}