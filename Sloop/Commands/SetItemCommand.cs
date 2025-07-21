namespace Sloop.Commands;

using Abstractions;
using Core;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Npgsql;

/// <summary>
///     Arguments for inserting or updating a cache item.
/// </summary>
/// <param name="Key">The cache key.</param>
/// <param name="Value">The cached value as a byte array.</param>
/// <param name="Options">The cache expiration options.</param>
public record SetItemArgs(string Key, byte[] Value, DistributedCacheEntryOptions? Options);

/// <summary>
///     Command to insert or update a cache item in PostgreSQL,
///     applying sliding and absolute expiration rules.
/// </summary>
public class SetItemCommand : IDbCacheCommand<SetItemArgs, bool>
{
    private readonly SloopOptions _options;

    private readonly TimeProvider _time;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SetItemCommand" /> class.
    /// </summary>
    /// <param name="options">The configured Sloop options.</param>
    /// <param name="time">The system time provider used for expiration calculations.</param>
    public SetItemCommand(IOptions<SloopOptions> options, TimeProvider time)
    {
        _options = options.Value;
        _time = time;
    }

    /// <inheritdoc />
    public async Task<bool> ExecuteAsync(NpgsqlConnection connection, SetItemArgs args, CancellationToken token = default)
    {
        var options = args.Options ?? new DistributedCacheEntryOptions();

        var now = _time.GetUtcNow();

        var absolute = GetAbsoluteExpiration(now, options);

        var sliding = options.SlidingExpiration ?? _options.DefaultSlidingExpiration;

        var expiration = CoerceExpiration(now, absolute, sliding);

        await using var cmd = connection.CreateCommand();

        cmd.CommandText =
            $"""
             INSERT INTO {_options.GetQualifiedTableName()} (key, value, expires_at, sliding_interval, absolute_expiry)
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

    private DateTimeOffset? GetAbsoluteExpiration(DateTimeOffset now, DistributedCacheEntryOptions options)
    {
        if (options.AbsoluteExpiration.HasValue)
        {
            return options.AbsoluteExpiration;
        }

        if (options.AbsoluteExpirationRelativeToNow.HasValue)
        {
            return now.Add(options.AbsoluteExpirationRelativeToNow.Value);
        }

        if (_options.DefaultSlidingExpiration.HasValue)
        {
            return now.Add(_options.DefaultSlidingExpiration.Value);
        }

        return null;
    }

    private DateTimeOffset? CoerceExpiration(DateTimeOffset now, DateTimeOffset? absolute, TimeSpan? sliding)
    {
        if (!sliding.HasValue)
        {
            return absolute;
        }

        var expiration = now.Add(sliding.Value);

        if (expiration > absolute)
        {
            return absolute;
        }

        return expiration;
    }
}