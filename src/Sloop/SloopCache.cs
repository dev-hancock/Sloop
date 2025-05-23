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
        await using var connection = _services.Connection.Create();

        return await _services.Operations.GetAsync(connection, key, token);
    }

    /// <inheritdoc />
    public void Refresh(string key)
    {
        RefreshAsync(key).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public async Task RefreshAsync(string key, CancellationToken token = new())
    {
        await using var connection = _services.Connection.Create();

        await _services.Operations.RefreshAsync(connection, key, token);
    }

    /// <inheritdoc />
    public void Remove(string key)
    {
        RemoveAsync(key).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string key, CancellationToken token = new())
    {
        await using var connection = _services.Connection.Create();

        await _services.Operations.RemoveAsync(connection, key, token);
    }

    /// <inheritdoc />
    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        SetAsync(key, value, options).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = new())
    {
        await using var connection = _services.Connection.Create();

        await _services.Operations.SetAsync(connection, key, value, options, token);
    }
}