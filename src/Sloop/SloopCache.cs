namespace Sloop;

using Commands;
using Interfaces;
using Microsoft.Extensions.Caching.Distributed;

/// <summary>
///     Implements <see cref="IDistributedCache" /> using PostgreSQL as the backing store.
///     Executes structured commands for common cache operations.
/// </summary>
public class SloopCache : IDistributedCache
{
    private readonly IDbConnectionFactory _connection;

    private readonly IDbCacheOperations _operations;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SloopCache" /> class.
    /// </summary>
    /// <param name="connection">Factory for creating PostgreSQL connections.</param>
    /// <param name="operations">The set of cache command implementations.</param>
    public SloopCache(IDbConnectionFactory connection, IDbCacheOperations operations)
    {
        _connection = connection;
        _operations = operations;
    }

    /// <inheritdoc />
    public byte[]? Get(string key)
    {
        return GetAsync(key).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public async Task<byte[]?> GetAsync(string key, CancellationToken token = new())
    {
        await using var connection = await _connection.Create(token);

        return await _operations.GetItem.ExecuteAsync(connection, new GetItemArgs(key), token);
    }

    /// <inheritdoc />
    public void Refresh(string key)
    {
        RefreshAsync(key).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public async Task RefreshAsync(string key, CancellationToken token = new())
    {
        await using var connection = await _connection.Create(token);
        await _operations.RefreshItem.ExecuteAsync(connection, new RefreshItemArgs(key), token);
    }

    /// <inheritdoc />
    public void Remove(string key)
    {
        RemoveAsync(key).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string key, CancellationToken token = new())
    {
        await using var connection = await _connection.Create(token);
        await _operations.RemoveItem.ExecuteAsync(connection, new RemoveItemArgs(key), token);
    }

    /// <inheritdoc />
    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        SetAsync(key, value, options).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = new())
    {
        await using var connection = await _connection.Create(token);
        await _operations.SetItem.ExecuteAsync(connection, new SetItemArgs(key, value, options), token);
    }
}