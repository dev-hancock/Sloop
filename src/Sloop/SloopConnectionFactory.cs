using Sloop.Commands;

namespace Sloop;

using Microsoft.Extensions.Options;
using Npgsql;

/// <summary>
/// Concrete implementation of <see cref="IDbConnectionFactory" /> that opens a connection and ensures the cache table
/// exists.
/// </summary>
public class SloopConnectionFactory : IDbConnectionFactory
{
    private readonly IDbCacheOperations _operations;
    
    private readonly object _lock = new();

    private readonly SloopOptions _options;

    private volatile bool _created;

    /// <summary>
    /// Constructs a new instance of <see cref="SloopConnectionFactory" />.
    /// </summary>
    public SloopConnectionFactory(IOptions<SloopOptions> options, IDbCacheOperations operations)
    {
        _operations = operations;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<NpgsqlConnection> Create(CancellationToken token = default)
    {
        var connection = new NpgsqlConnection(_options.ConnectionString);

        await connection.OpenAsync(token).ConfigureAwait(false);

        await EnsureTableCreated(connection, token);

        return connection;
    }

    /// <summary>
    /// Ensures the cache schema and table are created only once per process.
    /// Thread-safe using double-checked locking.
    /// </summary>
    private async Task EnsureTableCreated(NpgsqlConnection connection, CancellationToken token)
    {
        var create = false;

        lock (_lock)
        {
            if (!_created)
            {
                _created = create = true;
            }
        }

        if (create)
        {
            await _operations.CreateTable.ExecuteAsync(connection, null, token);
        }
    }
}