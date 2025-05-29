namespace Sloop;

using Interfaces;
using Microsoft.Extensions.Options;
using Npgsql;

/// <summary>
///     Provides a thread-safe implementation of <see cref="IDbConnectionFactory" /> that ensures
///     the cache table schema is created only once per process.
/// </summary>
public class SloopConnectionFactory : IDbConnectionFactory
{
    private readonly object _lock = new();

    private readonly IDbCacheOperations _operations;

    private readonly SloopOptions _options;

    private volatile bool _created;

    /// <summary>
    ///     Constructs a new instance of <see cref="SloopConnectionFactory" />.
    /// </summary>
    /// <param name="options">The configured Sloop options.</param>
    /// <param name="operations">The cache operations used to initialize the table.</param>
    public SloopConnectionFactory(IOptions<SloopOptions> options, IDbCacheOperations operations)
    {
        _operations = operations;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<NpgsqlConnection> Create(CancellationToken token = default)
    {
        var connection = _options.ConnectionFactory(_options.ConnectionString);

        await connection.OpenAsync(token).ConfigureAwait(false);

        await EnsureTableCreated(connection, token);

        return connection;
    }

    /// <summary>
    ///     Ensures the cache schema and table are created only once per process.
    ///     Thread-safe using double-checked locking.
    /// </summary>
    /// <param name="connection">The open PostgreSQL connection.</param>
    /// <param name="token">A cancellation token.</param>
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
            await _operations.CreateTable.ExecuteAsync(connection, null!, token);
        }
    }
}