using Sloop.Commands;

namespace Sloop;

using Microsoft.Extensions.Caching.Distributed;

public class SloopCache : IDistributedCache
{
    private readonly SloopServices _services;

    public SloopCache(SloopServices services)
    {
        _services = services;
    }

    /// <inheritdoc />
    public byte[]? Get(string key)
    {
        return GetAsync(key).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public async Task<byte[]?> GetAsync(string key, CancellationToken token = new())
    {
        await using var connection = await _services.Connection.Create(token);

        return await _services.Operations.GetItem.ExecuteAsync(connection, new GetItemArgs(key), token);
    }

    /// <inheritdoc />
    public void Refresh(string key)
    {
        RefreshAsync(key).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public async Task RefreshAsync(string key, CancellationToken token = new())
    {
        await using var connection = await _services.Connection.Create(token);
        
        await _services.Operations.RefreshItem.ExecuteAsync(connection, new RefreshItemArgs(key), token);
    }

    /// <inheritdoc />
    public void Remove(string key)
    {
        RemoveAsync(key).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string key, CancellationToken token = new())
    {
        await using var connection = await _services.Connection.Create(token);

        await _services.Operations.RemoveItem.ExecuteAsync(connection, new RemoveItemArgs(key), token);
    }

    /// <inheritdoc />
    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        SetAsync(key, value, options).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = new())
    {
        await using var connection = await _services.Connection.Create(token);

        await _services.Operations.SetItem.ExecuteAsync(connection, new SetItemArgs(key, value, options), token);
    }
}