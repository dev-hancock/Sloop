using Microsoft.Extensions.DependencyInjection;
using Sloop.Commands;

namespace Sloop;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Npgsql;

public interface IDbCacheOperations
{
    IDbCacheCommand<CreateTableArgs> CreateTable { get; }
    
    IDbCacheCommand<GetItemArgs, byte[]?> GetItem { get; }
    
    IDbCacheCommand<PurgeExpiredItemsArgs, long> PurgeExpiredItems { get; }
    
    IDbCacheCommand<RefreshItemArgs, bool> RefreshItem { get; }
    
    IDbCacheCommand<RemoveItemArgs, bool> RemoveItem { get; }
    
    IDbCacheCommand<SetItemArgs> SetItem { get; }
    
    IDbCacheCommand<TryAcquireLockArgs, bool> TryAcquireLock { get; }
}

/// <summary>
/// Implements PostgreSQL-backed cache operations for distributed caching.
/// Uses SQL command builders and expiration strategy to manage cache state.
/// </summary>
public class SloopOperations : IDbCacheOperations
{
    private readonly IServiceProvider _services;
    
    public SloopOperations(IServiceProvider services)
    {
        _services = services;
    }
    
    public IDbCacheCommand<CreateTableArgs> CreateTable => _services.GetRequiredService<IDbCacheCommand<CreateTableArgs>>();
    
    public IDbCacheCommand<GetItemArgs, byte[]?> GetItem => _services.GetRequiredService<IDbCacheCommand<GetItemArgs, byte[]?>>();
    
    public IDbCacheCommand<PurgeExpiredItemsArgs, long> PurgeExpiredItems => _services.GetRequiredService<IDbCacheCommand<PurgeExpiredItemsArgs, long>>();
    
    public IDbCacheCommand<RefreshItemArgs, bool> RefreshItem => _services.GetRequiredService<IDbCacheCommand<RefreshItemArgs, bool>>();
    
    public IDbCacheCommand<RemoveItemArgs, bool> RemoveItem => _services.GetRequiredService<IDbCacheCommand<RemoveItemArgs, bool>>();

    public IDbCacheCommand<SetItemArgs> SetItem => _services.GetRequiredService<IDbCacheCommand<SetItemArgs>>();
    
    public IDbCacheCommand<TryAcquireLockArgs, bool> TryAcquireLock => _services.GetRequiredService<IDbCacheCommand<TryAcquireLockArgs, bool>>();
}
