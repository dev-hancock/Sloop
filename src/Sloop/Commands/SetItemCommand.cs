using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Sloop.Commands;

public record SetItemArgs(string Key, byte[] Value, DistributedCacheEntryOptions? Options);

public class SetItemCommand : IDbCacheCommand<SetItemArgs, bool>
{
    private readonly TimeProvider _time;
    
    private readonly SloopOptions _options;

    public SetItemCommand(IOptions<SloopOptions> options, TimeProvider time)
    {
        _time = time;
        _options = options.Value;
    }

    private static DateTimeOffset? GetAbsoluteExpiration(DateTimeOffset now, DistributedCacheEntryOptions options)
    {
        if (options.AbsoluteExpiration.HasValue)
        {
            return options.AbsoluteExpiration;
        }

        if (options.AbsoluteExpirationRelativeToNow.HasValue)
        {
            return now.Add(options.AbsoluteExpirationRelativeToNow.Value);
        }

        return null;
    }

    private static DateTimeOffset? CoerceExpiration(DateTimeOffset now, DateTimeOffset? absolute, TimeSpan? sliding)
    {
        if (!sliding.HasValue)
        {
            return absolute;
        }

        var expiration = now.Add(sliding.Value);

        if (absolute.HasValue && expiration > absolute.Value)
        {
            return absolute;
        }

        return expiration; 
    }
    
    public async Task<bool> ExecuteAsync(NpgsqlConnection connection, SetItemArgs args, CancellationToken token = default)
    {
        var options = args.Options ?? new DistributedCacheEntryOptions();
        
        var now = _time.GetUtcNow();
        
        var absolute = GetAbsoluteExpiration(now, options);
        
        var sliding = options.SlidingExpiration ?? _options.DefaultExpiration;

        var expiration = CoerceExpiration(now, absolute, sliding);
        
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
        cmd.Parameters.AddWithValue("expires_at", expiration.HasValue ? expiration.Value.UtcDateTime : DBNull.Value);
        cmd.Parameters.AddWithValue("sliding_interval", sliding.HasValue ? sliding.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("absolute_expiry", absolute.HasValue ? absolute.Value.UtcDateTime : DBNull.Value);
        
        var result = await cmd.ExecuteNonQueryAsync(token);

        return result == 1;
    }
}