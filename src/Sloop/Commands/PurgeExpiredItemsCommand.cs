using Microsoft.Extensions.Options;
using Npgsql;

namespace Sloop.Commands;

public record PurgeExpiredItemsArgs(long Limit = 1000);

public class PurgeExpiredItemsCommand : IDbCacheCommand<PurgeExpiredItemsArgs, long>
{
    private readonly SloopOptions _options;

    public PurgeExpiredItemsCommand(IOptions<SloopOptions> options)
    {
       _options = options.Value;
    }
    
    public async Task<long> ExecuteAsync(NpgsqlConnection connection, PurgeExpiredItemsArgs args, CancellationToken token = default)
    {
        var total = 0L;

        while (!token.IsCancellationRequested)
        {
            await using var cmd = connection.CreateCommand();

            cmd.CommandText =
                $"""
                 DELETE FROM "{_options.SchemaName}"."{_options.TableName}"
                 WHERE ctid IN (
                     SELECT ctid FROM "{_options.SchemaName}"."{_options.TableName}"
                     WHERE expires_at <= now()
                     LIMIT {args.Limit}
                 );
                 """;
            
            var count = await cmd.ExecuteNonQueryAsync(token);

            if (count == 0)
            {
                break;
            }

            total += count;
        }

        return total; 
    }
}