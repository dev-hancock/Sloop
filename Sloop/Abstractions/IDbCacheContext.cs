namespace Sloop.Abstractions;

using Microsoft.Extensions.Caching.Distributed;

public interface IDbCacheContext
{
    /// <summary>
    ///     Retrieves the cached value for the given key.
    /// </summary>
    /// <param name="key">The cache key to look up.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task<byte[]?> GetAsync(string key, CancellationToken ct = default);

    /// <summary>
    ///     Refreshes the sliding expiration for the given cache key.
    /// </summary>
    /// <param name="key">The cache key to refresh.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task RefreshAsync(string key, CancellationToken ct = default);

    /// <summary>
    ///     Removes the cache entry for the given key.
    /// </summary>
    /// <param name="key">The cache key to remove.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task RemoveAsync(string key, CancellationToken ct = default);

    /// <summary>
    ///     Inserts or updates the cache entry for the given key and value.
    /// </summary>
    /// <param name="key">The cache key to set.</param>
    /// <param name="value">The binary payload to store.</param>
    /// <param name="options">Expiration options for the cache entry.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken ct = default);

    /// <summary>
    ///     Applies any pending database migrations for the cache schema and table.
    /// </summary>
    /// <param name="ct">A token to cancel the migration.</param>
    Task MigrateAsync(CancellationToken ct = default);

    /// <summary>
    ///     Purges expired entries from the cache table.
    /// </summary>
    /// <param name="ct">A token to cancel the purge operation.</param>
    Task CleanupAsync(CancellationToken ct = default);
}