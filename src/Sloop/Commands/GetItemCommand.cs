using Microsoft.Extensions.Options;
using Npgsql;

namespace Sloop.Commands;

public record GetItemArgs(string Key);

public class GetItemCommand : IDbCacheCommand<GetItemArgs, byte[]?>
{
    private readonly SloopOptions _options;

    public GetItemCommand(IOptions<SloopOptions> options)
    {
        _options = options.Value;
    }
    
    public async Task<byte[]?> ExecuteAsync(NpgsqlConnection connection, GetItemArgs args, CancellationToken token = default)
    {
        await using var cmd = connection.CreateCommand();

        cmd.CommandText =
            $"""
             UPDATE "{_options.SchemaName}"."{_options.TableName}"
             SET expires_at = LEAST(now() + sliding_interval, absolute_expiry)
             WHERE key = @key
               AND (expires_at IS NULL OR expires_at > now())
               AND sliding_interval IS NOT NULL
             RETURNING value;
             """;

        cmd.Parameters.AddWithValue("key", args.Key);    
        
        var result = await cmd.ExecuteScalarAsync(token);

        return result == DBNull.Value || result is null ? null : (byte[])result; 
    }
}