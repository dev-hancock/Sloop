namespace Sloop;

using Microsoft.Extensions.Options;
using Npgsql;

/// <summary>
/// Abstraction for creating new PostgreSQL connections.
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    /// Creates and returns a new open <see cref="NpgsqlConnection" />.
    /// </summary>
    NpgsqlConnection Create();
}

/// <summary>
/// Concrete implementation of <see cref="IDbConnectionFactory" /> that opens a connection and ensures the cache table
/// exists.
/// </summary>
public class SloopConnectionFactory : IDbConnectionFactory
{
    private static readonly object Lock = new();

    private readonly SloopOptions _options;

    private volatile bool _created;

    /// <summary>
    /// Constructs a new instance of <see cref="SloopConnectionFactory" />.
    /// </summary>
    public SloopConnectionFactory(IOptions<SloopOptions> options)
    {
        _options = options.Value;
    }

    /// <inheritdoc />
    public NpgsqlConnection Create()
    {
        var connection = new NpgsqlConnection(_options.ConnectionString);

        connection.Open();

        EnsureTableCreated(connection);

        return connection;
    }

    /// <summary>
    /// Ensures the cache schema and table are created only once per process.
    /// Thread-safe using double-checked locking.
    /// </summary>
    private void EnsureTableCreated(NpgsqlConnection connection)
    {
        if (_created)
        {
            return;
        }

        lock (Lock)
        {
            if (_created)
            {
                return;
            }

            using var cmd = SloopCommands.CreateTable(connection, _options.SchemaName, _options.TableName);

            cmd.ExecuteNonQuery();

            _created = true;
        }
    }
}