namespace Sloop;

using Commands;
using Interfaces;

/// <summary>
///     Implements PostgreSQL-backed cache operations for distributed caching.
///     Uses SQL command builders and expiration strategy to manage cache state.
/// </summary>
public class SloopOperations : IDbCacheOperations
{
    private readonly IDbCommandResolver _resolver;

    public SloopOperations(IDbCommandResolver resolver)
    {
        _resolver = resolver;
    }

    /// <inheritdoc />
    public IDbCacheCommand<CreateTableArgs, bool> CreateTable => _resolver.Resolve<CreateTableArgs, bool>();

    /// <inheritdoc />
    public IDbCacheCommand<GetItemArgs, byte[]?> GetItem => _resolver.Resolve<GetItemArgs, byte[]?>();

    /// <inheritdoc />
    public IDbCacheCommand<PurgeExpiredItemsArgs, long> PurgeExpiredItems => _resolver.Resolve<PurgeExpiredItemsArgs, long>();

    /// <inheritdoc />
    public IDbCacheCommand<RefreshItemArgs, bool> RefreshItem => _resolver.Resolve<RefreshItemArgs, bool>();

    /// <inheritdoc />
    public IDbCacheCommand<RemoveItemArgs, bool> RemoveItem => _resolver.Resolve<RemoveItemArgs, bool>();

    /// <inheritdoc />
    public IDbCacheCommand<SetItemArgs, bool> SetItem => _resolver.Resolve<SetItemArgs, bool>();

    /// <inheritdoc />
    public IDbCacheCommand<TryAcquireLockArgs, bool> TryAcquireLock => _resolver.Resolve<TryAcquireLockArgs, bool>();
}