namespace Sloop;

/// <summary>
/// Container for cache-related services required by the PostgreSQL cache implementation.
/// </summary>
public class SloopServices
{
    /// <summary>
    /// Constructs a new instance of <see cref="SloopServices" />.
    /// </summary>
    public SloopServices(IDbCacheOperations operations, IDbConnectionFactory connection)
    {
        Operations = operations;
        Connection = connection;
    }

    /// <summary>
    /// Provides access to PostgreSQL connection creation logic.
    /// </summary>
    public IDbConnectionFactory Connection { get; }

    /// <summary>
    /// Provides access to distributed cache operations for PostgreSQL.
    /// </summary>
    public IDbCacheOperations Operations { get; }
}