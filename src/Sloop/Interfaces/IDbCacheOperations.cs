namespace Sloop
{
    using Commands;

    public interface IDbCacheOperations
    {
        IDbCacheCommand<CreateTableArgs, bool> CreateTable { get; }
    
        IDbCacheCommand<GetItemArgs, byte[]?> GetItem { get; }
    
        IDbCacheCommand<PurgeExpiredItemsArgs, long> PurgeExpiredItems { get; }
    
        IDbCacheCommand<RefreshItemArgs, bool> RefreshItem { get; }
    
        IDbCacheCommand<RemoveItemArgs, bool> RemoveItem { get; }
    
        IDbCacheCommand<SetItemArgs, bool> SetItem { get; }
    
        IDbCacheCommand<TryAcquireLockArgs, bool> TryAcquireLock { get; }
    }
}