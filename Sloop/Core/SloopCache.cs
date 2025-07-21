namespace Sloop.Core;

using Abstractions;
using Microsoft.Extensions.Caching.Distributed;

/// <summary>
///     Implements <see cref="IDistributedCache" /> using PostgreSQL as the backing store.
///     Executes structured commands for common cache operations.
/// </summary>
public class SloopCache : IDistributedCache
{
    private readonly IDbCacheContext _context;


    /// <summary>
    ///     Initializes a new instance of the <see cref="SloopCache" /> class.
    /// </summary>
    public SloopCache(IDbCacheContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public byte[]? Get(string key)
    {
        return GetAsync(key).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public Task<byte[]?> GetAsync(string key, CancellationToken ct = default)
    {
        return _context.GetAsync(key, ct);
    }

    /// <inheritdoc />
    public void Refresh(string key)
    {
        RefreshAsync(key).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public Task RefreshAsync(string key, CancellationToken ct = default)
    {
        return _context.RefreshAsync(key, ct);
    }

    /// <inheritdoc />
    public void Remove(string key)
    {
        RemoveAsync(key).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        return _context.RemoveAsync(key, ct);
    }

    /// <inheritdoc />
    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        SetAsync(key, value, options).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken ct = default)
    {
        return _context.SetAsync(key, value, options, ct);
    }
}