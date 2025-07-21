namespace Sloop.Core;

using Abstractions;
using Commands;

/// <summary>
///     Implements PostgreSQL-backed cache operations for distributed caching.
///     Uses SQL command builders and expiration strategy to manage cache state.
/// </summary>
public class SloopOperations : IDbCacheOperations
{
    private readonly IDbCommandFactory _factory;

    public SloopOperations(IDbCommandFactory factory)
    {
        _factory = factory;
    }

    /// <inheritdoc />
    public IDbCacheCommand<GetItemArgs, byte[]?> GetItem => _factory.Resolve<GetItemArgs, byte[]?>();

    /// <inheritdoc />
    public IDbCacheCommand<PurgeExpiredItemsArgs, long> PurgeExpiredItems => _factory.Resolve<PurgeExpiredItemsArgs, long>();

    /// <inheritdoc />
    public IDbCacheCommand<RefreshItemArgs, bool> RefreshItem => _factory.Resolve<RefreshItemArgs, bool>();

    /// <inheritdoc />
    public IDbCacheCommand<RemoveItemArgs, bool> RemoveItem => _factory.Resolve<RemoveItemArgs, bool>();

    /// <inheritdoc />
    public IDbCacheCommand<SetItemArgs, bool> SetItem => _factory.Resolve<SetItemArgs, bool>();

    /// <inheritdoc />
    public IDbCacheCommand<TryAcquireLockArgs, bool> TryAcquireLock => _factory.Resolve<TryAcquireLockArgs, bool>();
}