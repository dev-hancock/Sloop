namespace Sloop.Commands;

using Interfaces;
using Microsoft.Extensions.Options;
using Npgsql;

/// <summary>
///     Arguments for purging expired items from the cache.
/// </summary>
/// <param name="Limit">Maximum number of items to delete per batch.</param>
public record PurgeExpiredItemsArgs(long Limit = 1000);

/// <summary>
///     Command to delete expired cache items from PostgreSQL in batches.
/// </summary>
public class PurgeExpiredItemsCommand : IDbCacheCommand<PurgeExpiredItemsArgs, long>
{
    private readonly SloopOptions _options;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PurgeExpiredItemsCommand" /> class.
    /// </summary>
    /// <param name="options">The configured Sloop options.</param>
    public PurgeExpiredItemsCommand(IOptions<SloopOptions> options)
    {
        _options = options.Value;
    }

    /// <inheritdoc />
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