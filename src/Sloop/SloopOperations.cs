namespace Sloop;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Npgsql;

/// <summary>
/// Defines low-level cache operations using a PostgreSQL backend.
/// These operations are invoked by higher-level cache abstractions such as IDistributedCache.
/// </summary>
public interface IDbCacheOperations
{
    /// <summary>
    /// Retrieves a cache entry by key if it exists and is not expired.
    /// </summary>
    Task<byte[]?> GetAsync(NpgsqlConnection connection, string key, CancellationToken token = default);

    /// <summary>
    /// Deletes expired cache entries in batches until none remain.
    /// Intended to be called periodically.
    /// </summary>
    Task<long> PurgeExpired(NpgsqlConnection connection, CancellationToken token = default);

    /// <summary>
    /// Refreshes the expiration of a cache entry based on the original sliding expiration interval.
    /// </summary>
    Task<bool> RefreshAsync(NpgsqlConnection connection, string key, CancellationToken token = default);

    /// <summary>
    /// Removes a cache entry by key.
    /// </summary>
    Task<bool> RemoveAsync(NpgsqlConnection connection, string key, CancellationToken token = default);

    /// <summary>
    /// Inserts or updates a cache entry with the specified value and expiration options.
    /// </summary>
    Task SetAsync(NpgsqlConnection connection, string key, byte[] value, DistributedCacheEntryOptions? options, CancellationToken token = default);

    /// <summary>
    /// Attempts to acquire a PostgreSQL advisory lock using the specified ID.
    /// Useful for coordinating background purge or refresh tasks.
    /// </summary>
    Task<bool> TryAcquireLock(NpgsqlConnection connection, long id, CancellationToken token = default);
}

/// <summary>
/// Implements PostgreSQL-backed cache operations for distributed caching.
/// Uses SQL command builders and expiration strategy to manage cache state.
/// </summary>
public class SloopOperations : IDbCacheOperations
{
    private readonly SloopOptions _options;

    private readonly TimeProvider _time;

    /// <summary>
    /// Constructs a new instance of <see cref="SloopOperations" />.
    /// </summary>
    public SloopOperations(IOptions<SloopOptions> options, TimeProvider time)
    {
        _time = time;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<byte[]?> GetAsync(NpgsqlConnection connection, string key, CancellationToken token = default)
    {
        await using var cmd = SloopCommands.GetItem(connection, _options.SchemaName, _options.TableName, key);

        var result = await cmd.ExecuteScalarAsync(token);

        return result == DBNull.Value || result is null ? null : (byte[])result;
    }

    /// <inheritdoc />
    public async Task<long> PurgeExpired(NpgsqlConnection connection, CancellationToken token = default)
    {
        var total = 0L;

        while (!token.IsCancellationRequested)
        {
            await using var purge = SloopCommands.PurgeExpired(connection, _options.SchemaName, _options.TableName);

            var count = await purge.ExecuteNonQueryAsync(token);

            if (count == 0)
            {
                break;
            }

            total += count;
        }

        return total;
    }

    /// <inheritdoc />
    public async Task<bool> RefreshAsync(NpgsqlConnection connection, string key, CancellationToken token = default)
    {
        await using var cmd = SloopCommands.RefreshItem(connection, _options.SchemaName, _options.TableName, key);

        var count = await cmd.ExecuteNonQueryAsync(token);

        return count > 0;
    }

    /// <inheritdoc />
    public async Task<bool> RemoveAsync(NpgsqlConnection connection, string key, CancellationToken token = default)
    {
        await using var cmd = SloopCommands.RemoveItem(connection, _options.SchemaName, _options.TableName, key);

        var count = await cmd.ExecuteNonQueryAsync(token);

        return count > 0;
    }

    /// <inheritdoc />
    public async Task SetAsync(NpgsqlConnection connection, string key, byte[] value, DistributedCacheEntryOptions? options, CancellationToken token = default)
    {
        options ??= new DistributedCacheEntryOptions();

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

        await using var cmd = SloopCommands.SetItem(connection, _options.SchemaName, _options.TableName, key, value, expiry, sliding, absolute);

        await cmd.ExecuteNonQueryAsync(token);
    }

    /// <inheritdoc />
    public async Task<bool> TryAcquireLock(NpgsqlConnection connection, long id, CancellationToken token = default)
    {
        await using var sync = SloopCommands.TryAcquireLock(connection, id);

        var acquired = await sync.ExecuteScalarAsync(token);

        return acquired is true;
    }
}