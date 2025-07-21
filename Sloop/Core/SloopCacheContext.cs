namespace Sloop.Core;

using Abstractions;
using Commands;
using Microsoft.Extensions.Caching.Distributed;

/// <summary>
///     Provides a unit-of-work–style context for cache operations and migrations.
/// </summary>
public class SloopCacheContext : IDbCacheContext
{
    private readonly IDbConnectionFactory _connection;

    private readonly IDbCacheMigrator _migrator;

    private readonly IDbCacheOperations _operations;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SloopCacheContext" /> class.
    /// </summary>
    /// <param name="connection">The factory used to create database connections.</param>
    /// <param name="migrator">The migrator used to apply schema/table migrations.</param>
    /// <param name="operations">The set of database cache command operations.</param>
    public SloopCacheContext(IDbConnectionFactory connection, IDbCacheMigrator migrator, IDbCacheOperations operations)
    {
        _connection = connection;
        _migrator = migrator;
        _operations = operations;
    }

    /// <summary>
    ///     Retrieves the cached value for the specified key.
    /// </summary>
    /// <param name="key">The cache key to look up.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    public async Task<byte[]?> GetAsync(string key, CancellationToken ct = default)
    {
        await using var connection = await _connection.Create(ct);

        return await _operations.GetItem.ExecuteAsync(connection, new GetItemArgs(key), ct);
    }

    /// <summary>
    ///     Refreshes the sliding expiration for the specified cache key.
    /// </summary>
    /// <param name="key">The cache key to refresh.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    public async Task RefreshAsync(string key, CancellationToken ct = default)
    {
        await using var connection = await _connection.Create(ct);

        await _operations.RefreshItem.ExecuteAsync(connection, new RefreshItemArgs(key), ct);
    }

    /// <summary>
    ///     Removes the cache entry for the specified key.
    /// </summary>
    /// <param name="key">The cache key to remove.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        await using var connection = await _connection.Create(ct);

        await _operations.RemoveItem.ExecuteAsync(connection, new RemoveItemArgs(key), ct);
    }

    /// <summary>
    ///     Inserts or updates the cache entry for the specified key and value.
    /// </summary>
    /// <param name="key">The cache key to set.</param>
    /// <param name="value">The value to store as a byte array.</param>
    /// <param name="options">The expiration options for the cache entry.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken ct = default)
    {
        await using var connection = await _connection.Create(ct);

        await _operations.SetItem.ExecuteAsync(connection, new SetItemArgs(key, value, options), ct);
    }

    /// <summary>
    ///     Applies any pending database migrations for the cache schema and table.
    /// </summary>
    /// <param name="ct">A token to cancel the migration operation.</param>
    public Task MigrateAsync(CancellationToken ct = default)
    {
        return _migrator.MigrateAsync(ct);
    }

    /// <summary>
    ///     Attempts to purge expired cache entries if an advisory lock can be acquired.
    /// </summary>
    /// <param name="ct">A token to cancel the cleanup operation.</param>
    public async Task CleanupAsync(CancellationToken ct = default)
    {
        await using var connection = await _connection.Create(ct);

        if (await _operations.TryAcquireLock.ExecuteAsync(connection, new TryAcquireLockArgs(42_000), ct))
        {
            await _operations.PurgeExpiredItems.ExecuteAsync(connection, new PurgeExpiredItemsArgs(), ct);
        }
    }
}