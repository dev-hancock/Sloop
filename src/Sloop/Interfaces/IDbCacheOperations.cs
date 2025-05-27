namespace Sloop.Interfaces;

using Commands;

/// <summary>
///     Defines the set of database cache operations available to the cache implementation.
///     Each operation corresponds to a discrete database command.
/// </summary>
public interface IDbCacheOperations
{
    /// <summary>Gets the command for creating the cache table schema.</summary>
    IDbCacheCommand<CreateTableArgs, bool> CreateTable { get; }

    /// <summary>Gets the command for retrieving a cached item.</summary>
    IDbCacheCommand<GetItemArgs, byte[]?> GetItem { get; }

    /// <summary>Gets the command for purging expired cache items.</summary>
    IDbCacheCommand<PurgeExpiredItemsArgs, long> PurgeExpiredItems { get; }

    /// <summary>Gets the command for refreshing sliding expiration for a cached item.</summary>
    IDbCacheCommand<RefreshItemArgs, bool> RefreshItem { get; }

    /// <summary>Gets the command for removing a cached item by key.</summary>
    IDbCacheCommand<RemoveItemArgs, bool> RemoveItem { get; }

    /// <summary>Gets the command for inserting or updating a cached item.</summary>
    IDbCacheCommand<SetItemArgs, bool> SetItem { get; }

    /// <summary>Gets the command for acquiring a PostgreSQL advisory lock.</summary>
    IDbCacheCommand<TryAcquireLockArgs, bool> TryAcquireLock { get; }
}