using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Sloop.Commands;

public record SetItemArgs(string Key, byte[] Value, DistributedCacheEntryOptions? Options);

public class SetItemCommand : IDbCacheCommand<SetItemArgs>
{
    private readonly TimeProvider _time;
    
    private readonly SloopOptions _options;

    public SetItemCommand(IOptions<SloopOptions> options, TimeProvider time)
    {
        _time = time;
        _options = options.Value;
    }
    
    public async Task ExecuteAsync(NpgsqlConnection connection, SetItemArgs args, CancellationToken token = default)
    {
        var options = args.Options ?? new DistributedCacheEntryOptions();

        var absolute = options.AbsoluteExpiration;

        if (options.AbsoluteExpirationRelativeToNow.HasValue)
        {
            absolute = _time.GetUtcNow().Add(options.AbsoluteExpirationRelativeToNow.Value);
        }

        var sliding = options.SlidingExpiration ?? _options.DefaultExpiration;

        var expiry = absolute;

        if (sliding.HasValue)
        {
            expiry = _time.GetUtcNow().Add(sliding.Value);

            if (expiry > absolute)
            {
                expiry = absolute;
            }
        }
        
        await using var cmd = connection.CreateCommand();

        cmd.CommandText =
            $"""
             INSERT INTO "{_options.SchemaName}"."{_options.TableName}" (key, value, expires_at, sliding_interval, absolute_expiry)
             VALUES (@key, @value, @expires_at, @sliding_interval, @absolute_expiry)
             ON CONFLICT (key) DO UPDATE 
             SET value = @value,
                 expires_at = @expires_at,
                 sliding_interval = @sliding_interval,
                 absolute_expiry = @absolute_expiry;
             """;

        cmd.Parameters.AddWithValue("key", args.Key);
        cmd.Parameters.AddWithValue("value", args.Value);
        cmd.Parameters.AddWithValue("expires_at", expiry.HasValue ? expiry.Value.UtcDateTime : DBNull.Value);
        cmd.Parameters.AddWithValue("sliding_interval", sliding.HasValue ? sliding.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("absolute_expiry", absolute.HasValue ? absolute.Value.UtcDateTime : DBNull.Value);
        
        await cmd.ExecuteNonQueryAsync(token);
    }
}